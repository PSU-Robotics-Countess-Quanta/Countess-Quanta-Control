// Brad Pitney
// ECE 510
// Spring 2014

// RobotSpeech handles speech synthesis and robot mouth motion while speaking.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using System.Threading;

namespace CountessQuantaControl
{
    public class RobotSpeech
    {
        ServoManager servoManager;
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        EventWaitHandle waitHandle = new AutoResetEvent(false);
        bool isSpeaking = false;
        Object isSpeakingLock = new Object();
        long mouthServoIndex = -1;
        Random randomGenerator = new Random();

        public RobotSpeech(ServoManager servoManager)
        {
            this.servoManager = servoManager;
            mouthServoIndex = servoManager.GetServoIndex("Mouth open/close");

            speechSynthesizer.SelectVoice(speechSynthesizer.GetInstalledVoices().Where(i=>i.Enabled && i.VoiceInfo.Gender == VoiceGender.Female).First().VoiceInfo.Name);
            speechSynthesizer.Rate = 0;

            Thread speechAnimationThread = new Thread(new ThreadStart(SpeechAnimationThread));
            speechAnimationThread.IsBackground = true;
            speechAnimationThread.Start();

            speechSynthesizer.StateChanged += StateChanged;
        }

        // Starts speaking with the speech synthesizer.
        public void Speak(string speechString)
        {
            speechSynthesizer.SpeakAsyncCancelAll();
            speechSynthesizer.SpeakAsync(speechString);
        }

        // This is triggered when the speech synthesizer starts or stops speaking.
        private void StateChanged(object sender, StateChangedEventArgs e)
        {
            if (e.State == SynthesizerState.Speaking)
            {
                // Robot is speaking, so start the speech animation.
                lock (isSpeakingLock)
                {
                    isSpeaking = true;
                }

                waitHandle.Set();
            }
            else
            {
                // Robot is not speaking, so stop the speech animation.
                lock (isSpeakingLock)
                {
                    isSpeaking = false;
                }
            }
        }

        // Animates the robot's mouth while the speech synthesizer is speaking.
        private void SpeechAnimationThread()
        {
            while (true)
            {
                bool speaking = false;
                lock (isSpeakingLock)
                {
                    speaking = isSpeaking;
                }

                if (!speaking)
                {
                    // Pause the animation.
                    waitHandle.WaitOne();
                }


                // Perform the speech animation.

                const double mouthOpenPosition = 1200;
                const double mouthClosePosition = 700;
                const int minOpenPosition = (int)((mouthClosePosition + mouthOpenPosition) / 2);
                const double mouthMoveSpeedScaling = 0.1;
                const int mouthMoveDuration = 200;

                if (mouthServoIndex >= 0)
                {
                    // Prepare to open the mouth to a random position.
                    int randomOpenPosition = randomGenerator.Next(minOpenPosition, (int)mouthOpenPosition);

                    // Scale the speed based on the move distance.
                    long mouthMoveSpeed = (long)(((double)randomOpenPosition - mouthClosePosition) * mouthMoveSpeedScaling);

                    // Perform the mouth open and mouth close motions.
                    servoManager.StartSpeedMove(new ServoPosition(mouthServoIndex, randomOpenPosition), mouthMoveSpeed);
                    Thread.Sleep(mouthMoveDuration);
                    servoManager.StartSpeedMove(new ServoPosition(mouthServoIndex, mouthClosePosition), mouthMoveSpeed);
                    Thread.Sleep(mouthMoveDuration);
                }
            }
        }

        public event EventHandler<SpeakStartedEventArgs> SpeakStarted
        {
            add { speechSynthesizer.SpeakStarted += value; }
            remove { speechSynthesizer.SpeakStarted -= value; }
        }

        public event EventHandler<SpeakCompletedEventArgs> SpeakCompleted
        {
            add { speechSynthesizer.SpeakCompleted += value; }
            remove { speechSynthesizer.SpeakCompleted -= value; }
        }
    }
}
