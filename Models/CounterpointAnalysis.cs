using Melanchall;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.Models
{
    internal class CounterpointAnalysis
    {    
          
        TextFileLog log = new TextFileLog();        
        Piece Piece;
        CounterpointSpecie SelectedSpecie;
        List<string> CounterpointCommentsLog = new List<string>();
        

        public CounterpointAnalysis(string path, CounterpointSpecie selectedSpecie)
        {           
            MidiFile file = MidiFile.Read(path);
            Piece = new Piece(file);
            SelectedSpecie = selectedSpecie;

            foreach (var voiceline in Piece.VoiceLines)
            {
                voiceline.PopulateHorizontalIntervals();
            }            
            Piece.PopulateVerticalIntervals();

            //Logi do debugowania
            log.WriteLog(SelectedSpecie.ToUserFriendlyString());
            foreach (var a in Piece.VoiceLines[0].VerticalIntervals)
            {
                foreach(var b in a)
                {
                    log.WriteLog(b.ToString());
                }
            }

            RunCounterpointAnalysis(Piece, SelectedSpecie);

           
        } 
        private void RunCounterpointAnalysis(Piece piece, CounterpointSpecie specie)
        {

            //na razie zakladamy ze wszystko to 3 specie xddd
            foreach (var cantusFirmus in piece.VoiceLines)
            {

                for (int i = 0; i < piece.VoiceLines.Count && piece.VoiceLines[i].VoiceId != cantusFirmus.VoiceId; i++)
                {
                    VoiceLine voiceLine = piece.VoiceLines[i];
                    CheckCounterpointMode(cantusFirmus,voiceLine,specie);

                    CheckExposedTritones(cantusFirmus, voiceLine, specie);
                    CheckExposedFifths(cantusFirmus, voiceLine, specie);

                    //TODO check forbidden skips:
                    // aug fourth, seventh, octave+, descending minor sixths, major sixths.

                    //aug fourth
                    CheckSkip(cantusFirmus, voiceLine, specie, 6, 1, 1);
                    CheckSkip(cantusFirmus, voiceLine, specie, 6, -1, 1);

                    // seventh
                    CheckSkip(cantusFirmus, voiceLine, specie, 10, 1, 1);
                    CheckSkip(cantusFirmus, voiceLine, specie, 10, -1, 1);

                    CheckSkip(cantusFirmus, voiceLine, specie, 11, 1, 1);
                    CheckSkip(cantusFirmus, voiceLine, specie, 11, -1, 1);

                    //sixth
                    CheckSkip(cantusFirmus, voiceLine, specie, 9, 1, 1);
                    CheckSkip(cantusFirmus, voiceLine, specie, 9, -1, 1);
                    CheckSkip(cantusFirmus, voiceLine, specie, 8, -1, 1);

                    //Leaps bigger than octave
                    CheckSkipInequality(cantusFirmus, voiceLine, specie, 12, 1, 1, 1);
                    CheckSkipInequality(cantusFirmus, voiceLine, specie, 12, -1, 1, 1);

                    //TODO check for repeated notes
                    CheckSkip(cantusFirmus, voiceLine, specie, 0, 1, 1);

                    //TODO check for 2+ skips in the same direction

                    //TODO check perfect consonance(unison, fifth, octave) entrance


                    //TODO check closing formula
                    //allowed for upper cf:3m-5-4-3m-1
                    //allowed for upper cf:5-3m-1
                    //allowed for lower cf:8-7-5-6w-8
                    //allowed for lower cf:3-4-5-6w-8


                    //TODO check dissonnance (allowed in 2+ species)

                    //TODO check for parallel fifths/octaves

                    //TODO check for cambatia
                }
            }
        }
        private void CheckCounterpointMode(VoiceLine cantusFirmus ,VoiceLine voiceLine, CounterpointSpecie specie)
        {
            //checking for higher voice line
           
            //c.f. is the higher voice
            if (cantusFirmus.VerticalIntervals[voiceLine.VoiceId].Any(x => x < 0))
            {
                if (cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] == 12 ||
                    cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] == 0)
                {

                }
                else
                {
                    CounterpointCommentsLog
                        .Add("Mode has been changed- first interval should be octave or unison.");
                }
            }
            //c.f. is the lower voice
            else
            {
                if (cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] == 12 ||
                 cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] == 0 ||
                 cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] == 7)
                {

                }
                else
                {
                    CounterpointCommentsLog
                        .Add("Mode has been changed- first interval should be octave, fifth or unison.");
                }

            }

        }
        private void CheckExposedTritones(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie)
        {
            CheckSkip(cantusFirmus, voiceLine, specie, 6, 1, 2);
            CheckSkip(cantusFirmus, voiceLine, specie, 6, -1, 2);

        }
        private void CheckExposedFifths(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie)
        {
            CheckSkip(cantusFirmus, voiceLine, specie, 7, 1, 2);
            CheckSkip(cantusFirmus, voiceLine, specie, 7, -1, 2);

        }
        /// <summary>
        /// Function checking voice line for given interval skip
        /// </summary>
        /// <param name="cantusFirmus">cantusFirmus</param>
        /// <param name="voiceLine">voiceLine</param>
        /// <param name="specie">specie</param>
        /// <param name="skip">checked interval</param>
        /// <param name="skipDirection">pass 1 for upward motion, -1 for downward</param>
        /// <param name="NoteDistance">distane between 2 checked notes</param>
        private void CheckSkip(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie,
                                     int interval, int skipDirection, int NoteDistance)
        {
            for (int i = 0; Math.Max(i,i + NoteDistance)< cantusFirmus.NotesUngrouped.Count; i++)
            {

                if (skipDirection * (cantusFirmus.NotesUngrouped[i].NoteNumber - cantusFirmus.NotesUngrouped[i + 2].NoteNumber) 
                    == interval)
                {
                    CounterpointCommentsLog.Add("Forbidden interval ("
                                                + GetNamedInterval(interval)
                                                + ") between notes "
                                                + i.ToString() + ", " + (i + NoteDistance).ToString());                                               
                }
            }
        }

        /// <summary>
        /// checks for skips larger/smaller than given interval.
        /// </summary>
        /// <param name="cantusFirmus">cantusFirmus</param>
        /// <param name="voiceLine">voiceLine</param>
        /// <param name="specie">specie</param>
        /// <param name="interval">checked interval</param>
        /// <param name="skipDirection">pass 1 for upward motion, -1 for downward</param>
        /// <param name="NoteDistance">distane between 2 checked notes</param>
        /// <param name="inequalityType">pass -1 for smaller than relation, 1 for larger than relation</param>
        private void CheckSkipInequality(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie,
                             int interval, int skipDirection, int NoteDistance, int inequalityType)
        {
            for (int i = 0; Math.Max(i, i + NoteDistance) < cantusFirmus.NotesUngrouped.Count; i++)
            {
                if (inequalityType * skipDirection * (cantusFirmus.NotesUngrouped[i].NoteNumber - cantusFirmus.NotesUngrouped[i + 2].NoteNumber)
                    > inequalityType * interval)
                {
                    CounterpointCommentsLog.Add("Forbidden interval ("
                                                + GetNamedInterval(interval)
                                                + ") between notes "
                                                + i.ToString() + ", " + (i + NoteDistance).ToString());
                }
            }
        }


        public static int GetIntervalInSemitones(Note a, Note b)
        {
                int interval =  b.NoteNumber - a.NoteNumber;
                return interval;                    
        }
        public static NamedInterval GetNamedInterval(Note a, Note b)
        {
           int interval = GetIntervalInSemitones(a, b);
            return (NamedInterval)interval;
        }
        public static NamedInterval GetNamedInterval(int interval)
        {
            return (NamedInterval)interval;
        }
    }

    public enum NamedInterval
    {
        Unison,
        MinorSecond,
        MajorSecond,
        MinorThird,
        MajorThird,
        PerfectFourth,
        Tritone,
        PerfectFifth,
        MinorSixth,
        MajorSixth,
        MinorSeventh,
        MajorSeventh,
        Octave,
        MinorNinth,
        MajorNinth,
        MinorTenth,
        MajorTenth,
        PerfectEleventh,
        AugmentedEleventh,
        PerfectTwelfth,
        MinorThirteenth,
        MajorThirteenth,
        MinorFourteenth,
        MajorFourteenth,
        DoubleOctave
    }

    public enum CounterpointSpecie
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth
    }
    public static class CounterpointSpecieExtensions
    {
        public static string ToUserFriendlyString(this CounterpointSpecie specie)
        {
            switch (specie)
            {
                case CounterpointSpecie.First:
                    return "First spiecie";
                case CounterpointSpecie.Second:
                    return "Second spiecie";
                case CounterpointSpecie.Third:
                    return "Third spiecie";
                case CounterpointSpecie.Fourth:
                    return "Fourth spiecie";
                case CounterpointSpecie.Fifth:
                    return "Fifth spiecie";
                default:
                    return "";
            }
        }
        public static CounterpointSpecie FromUserFriendlyString(this string specie)
        {
            switch (specie)
            {
                case "First spiecie":
                    return CounterpointSpecie.First;
                   
                case "Second spiecie":
                    return CounterpointSpecie.Second;
                case "Third spiecie":
                    return CounterpointSpecie.Third;
                case "Fourth spiecie":
                    return CounterpointSpecie.Fourth;
                case "Fifth spiecie":
                    return CounterpointSpecie.Fifth;
                default:
                    return CounterpointSpecie.First;
            }
        }
    }

    internal class Bar
    {
        public List<Note> notes { get; set; }
     
        public Note this[int i]
        {
            get
            {
                return notes[i];
            } 
        }

        public Bar()
        {
            notes = new List<Note>();           
        }

    }

    /// <summary>
    /// Single voice line composed of collection of bars
    /// </summary>
    internal class VoiceLine
    {
        public List<Note> NotesUngrouped;
        public List<Bar> Bars { get; set; }
        public int VoiceId { get; set; }
        public List<int> HorizontalIntervals;
        /// <summary>
        /// VerticalIntervals[i] represents
        /// intervals between current and VoiceLines[i]
        /// </summary>
        public List<List<int>> VerticalIntervals;



        public VoiceLine(List<Bar> bars, int voiceId, List<Note> notes)
        {
            this.Bars = bars;
            VoiceId = voiceId;
            NotesUngrouped = notes;
        }

        /// <summary>
        /// Indexer for Bars
        /// </summary>
        
        public Bar this[int i]
        {
            get
            {
                return Bars[i];
            }
        }


        public void PopulateHorizontalIntervals()
        {
            this.HorizontalIntervals = new List<int>();

            for(int j = 0; j<Bars.Count; j++)
            {
                for (int i = 0; i < Bars[j].notes.Count; i++)
                {
                    if (i + 1 < Bars[j].notes.Count)
                    {
                        HorizontalIntervals.Add(CounterpointAnalysis.GetIntervalInSemitones(Bars[j].notes[i], Bars[j].notes[i + 1]));
                    }
                    else
                    {
                        if(j+1<Bars.Count)
                        HorizontalIntervals.Add(CounterpointAnalysis.GetIntervalInSemitones(Bars[j].notes[i], Bars[j+1].notes[0]));
                    }
                }

            }

        }

    }


    // Measure is by deafault 4/4
    internal class Piece
    {
       public List<VoiceLine> VoiceLines {get; set;}     
       public long tempo { get; set; }
        TempoMap tempoMap;

        /// <summary>
        /// represents vertical intervals between voice lines
        /// fore example, VerticalIntervals[2][1] represents
        /// interval between second note and a corresponding
        /// note of third voice line.
        /// </summary>
        

        public Piece(MidiFile midiFile)
        {
           tempo = GetTempo(midiFile);
           VoiceLines = GetVoices(midiFile);
           tempoMap = midiFile.GetTempoMap();
        }

        private long GetTempo(MidiFile midiFile)
        {
            TempoMap tempoMap = midiFile.GetTempoMap();
            long ticks = 123;
            
            BarBeatTimeSpan barBeatTimeSpan = TimeConverter.ConvertTo<BarBeatTimeSpan>(ticks, tempoMap);
            MusicalTimeSpan musicalTimeSpan = TimeConverter.ConvertTo<MusicalTimeSpan>(ticks, tempoMap);

            return barBeatTimeSpan.Bars;           
        }

        private List<VoiceLine> GetVoices(MidiFile midiFile)
        {
            List<VoiceLine> voiceLines = new List<VoiceLine>();
            TempoMap tempoMap = midiFile.GetTempoMap();
            var tracks = midiFile.GetTrackChunks();

            int i = 0;

            foreach(var track in tracks)
            {
                           
                var notes = (List<Note>)track.GetNotes();                   
                voiceLines.Add(new VoiceLine(GetBars(notes,tempoMap),i, notes));
                i++;
            }
            return voiceLines;
        }

        List<Bar> GetBars(List<Note> notes, TempoMap tempoMap)
        {
            TextFileLog log = new TextFileLog();
            List<Bar> bars = new List<Bar>();
     
            long summedLenghtOfNotes = 0;
            int i = 0;
            Bar bar = new Bar();

            while (i<notes.Count)
            {
                

                var musicalTime = notes[i].LengthAs<MusicalTimeSpan>(tempoMap);            
                summedLenghtOfNotes += musicalTime.Numerator;

                if (summedLenghtOfNotes >= musicalTime.Denominator)
                {
                    bars.Add(bar);
                    bar = new Bar();
                    summedLenghtOfNotes = 0;
                }
                bar.notes.Add(notes[i]);
                i++;
            }
           
            bars.Add(bar);

            return bars;
        }


        public void PopulateVerticalIntervals()
        {
            int voiceNumber = VoiceLines.Count;

            for(int i = 0; i<voiceNumber; i++)
            {
                VoiceLines[i].VerticalIntervals = new List<List<int>>();

                for (int j = 0; j < voiceNumber; j++)
                {                   
                    SetVerticalIntervals(VoiceLines[i], VoiceLines[j],j);
                }
            }

        }

        private void SetVerticalIntervals(VoiceLine firstVoice, VoiceLine secondVoice, int voiceNumber)
        {
            long currentPositionInBar1 = 0;
            long currentPositionInBar2 = 0;

            firstVoice.VerticalIntervals.Add(new List<int>());

       
            for (int i = 0; i<firstVoice.Bars.Count; i++)
            {
              if(i<secondVoice.Bars.Count)
                {
                    int j = 0; // iterates over fist voices notes
                    int k = 0; // itarates over second voices notes
                    while(j < firstVoice[i].notes.Count && k< secondVoice[i].notes.Count)
                    {                         
                            firstVoice.VerticalIntervals[voiceNumber]
                                      .Add(CounterpointAnalysis.GetIntervalInSemitones(firstVoice[i][j], secondVoice[i][k]));
                    

                        var a = firstVoice[i][j].LengthAs<MusicalTimeSpan>(tempoMap);
                        currentPositionInBar1 += a.Numerator;

                        var b = secondVoice[i][k].LengthAs<MusicalTimeSpan>(tempoMap);
                        currentPositionInBar2 += b.Numerator;

                        if(currentPositionInBar1<currentPositionInBar2)
                        {
                            j++;
                        }
                        else if(currentPositionInBar1 > currentPositionInBar2)
                        
                        {
                            k++;
                        }
                        else
                        {
                            k++;
                            j++;
                        }
                                              
                    }
                }
            }
        }
    }

   
}
