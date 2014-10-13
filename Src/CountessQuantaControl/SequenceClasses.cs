// Brad Pitney
// ECE 579
// Winter 2014

// This file contains classes that are used for holding sequence data, and for
// loading/storing this data in xml files.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace CountessQuantaControl
{
    // A ServoPosition consists of an index specifying the servo and a new
    // target position for this servo.
    public class ServoPosition
    {
        [XmlAttribute("Index")]
        public long index;

        [XmlAttribute("Position")]
        public double position;

        public ServoPosition()
        {
        }

        public ServoPosition(long index, double position)
        {
            this.index = index;
            this.position = position;
        }
    }

    // A Frame contains a command to speak with the synthesizer, delay for a 
    // length of time, or move a set of servos, or some combination of these.
    public class Frame
    {
        [XmlElement("ServoPosition")]
        public List<ServoPosition> servoPositionList = new List<ServoPosition>();

        [XmlAttribute("Name")]
        public string name;

        [XmlAttribute("TimeToDestination")]
        public double timeToDestination;

        [XmlAttribute("Speech")]
        public string speechString;

        [XmlAttribute("Delay")]
        public double delay;

        public Frame()
        {

        }

        public Frame(string name)
        {
            this.name = name;
        }

        public void AddServoPosition(ServoPosition servoPosition)
        {
            servoPositionList.Add(servoPosition);
        }

        public List<ServoPosition> GetServoPositions()
        {
            return servoPositionList;
        }
    }

    // A Sequence contains an ordered list of Frames.
    public class Sequence
    {
        [XmlElement("Frame")]
        public List<Frame> frameList = new List<Frame>();

        [XmlAttribute("Name")]
        public string name;

        public Sequence()
        {

        }

        public Sequence(string name)
        {
            this.name = name;
        }

        public void AddFrame(Frame newFrame)
        {
            frameList.Add(newFrame);
        }

        public List<Frame> GetFrames()
        {
            return frameList;
        }
    }

    // A SequenceList contains the set of sequences that are stored in an xml file.
    public class SequenceList
    {
        List<Sequence> sequenceList = new List<Sequence>();

        public SequenceList()
        {
        }

        public SequenceList(string sequenceFileName)
        {
            LoadFromXml(sequenceFileName);
        }

        public void AddSequence(Sequence newSequence)
        {
            sequenceList.Add(newSequence);
        }

        public List<Sequence> GetSequences()
        {
            return sequenceList;
        }

        public void SaveToXml(string fileName)
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(List<Sequence>));
            TextWriter WriteFileStream = new StreamWriter(fileName);
            SerializerObj.Serialize(WriteFileStream, sequenceList);
            WriteFileStream.Close();
        }

        public void LoadFromXml(string fileName)
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(List<Sequence>));
            FileStream ReadFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            sequenceList = (List<Sequence>)SerializerObj.Deserialize(ReadFileStream);
            ReadFileStream.Close();
        }
    }
}
