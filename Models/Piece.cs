using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.Models
{
    // Measure is by deafault 4/4
    internal class Piece
    {
        public List<VoiceLine> VoiceLines { get; set; }
        public long tempo { get; set; }
        TempoMap tempoMap;
        List<long> NoteRythmicValues = new List<long>{
            1,
            2,
            4,
            8,
            16,
            32,
            64
        };
        

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
           
            foreach (var track in tracks)
            {

                var notes = (List<Melanchall.DryWetMidi.Smf.Interaction.Note>)track.GetNotes();
                if(notes.Count>0)
                {
                    voiceLines.Add(new VoiceLine(GetBars(notes, tempoMap), i, notes));
                }
                
                i++;
            }
            return voiceLines;
        }

        List<Bar> GetBars(List<Melanchall.DryWetMidi.Smf.Interaction.Note> notes, TempoMap tempoMap)
        {
            TextFileLog log = new TextFileLog();
            List<Bar> bars = new List<Bar>();

            long summedLenghtOfNotes = 0;
            int i = 0;
            Bar bar = new Bar();

            while (i < notes.Count)
            {              
                var musicalTime = notes[i].LengthAs<MusicalTimeSpan>(tempoMap);                                 
                summedLenghtOfNotes += RoundUpNoteLength(musicalTime.Numerator, musicalTime.Denominator);
                bar.notes.Add(notes[i]);
                if (summedLenghtOfNotes >= musicalTime.Denominator)
                {
                    bars.Add(bar);
                    bar = new Bar();
                    summedLenghtOfNotes = 0;
                }
               
               
                i++;
            }

            return bars;
        }
        private long RoundUpNoteLength(long time, long denominator)
        {
            int i = 0;
            while(i<NoteRythmicValues.Count && time<=denominator/NoteRythmicValues[i])
            {
                i++;
            }
            if (i == 0)
                return time;

            i--;
            return denominator / NoteRythmicValues[i];
        }

        public void PopulateVerticalIntervals()
        {
            int voiceNumber = VoiceLines.Count;

            for (int i = 0; i < voiceNumber; i++)
            {
                VoiceLines[i].VerticalIntervals = new List<List<int>>();

                for (int j = 0; j < voiceNumber; j++)
                {
                    SetVerticalIntervals(VoiceLines[i], VoiceLines[j], j);
                }
            }

        }

        private void SetVerticalIntervals(VoiceLine firstVoice, VoiceLine secondVoice, int voiceNumber)
        {
            long currentPositionInBar1 = 0;
            long currentPositionInBar2 = 0;

            firstVoice.VerticalIntervals.Add(new List<int>());

            TextFileLog log = new TextFileLog();
           

            for (int i = 0; i < firstVoice.Bars.Count; i++)
            {
                if (i < secondVoice.Bars.Count)
                {
                    int j = 0; // iterates over fist voices notes
                    int k = 0; // itarates over second voices notes

                    //variables keeping record of last value of j and k
                    int lastJ = -1;
                    int lastK = -1;
                    

                    while (j < firstVoice[i].notes.Count && k < secondVoice[i].notes.Count)
                    {

                        firstVoice.VerticalIntervals[voiceNumber]
                                  .Add(CounterpointAnalysis.GetIntervalInSemitones(firstVoice[i][j], secondVoice[i][k]));
                  


                        if(lastJ != j)
                        {
                        var a = firstVoice[i][j].LengthAs<MusicalTimeSpan>(tempoMap);
                        currentPositionInBar1 += a.Numerator%2 ==0 ? a.Numerator : a.Numerator+1;
                        }
                        if (lastK != k)
                        {
                            var b = secondVoice[i][k].LengthAs<MusicalTimeSpan>(tempoMap);
                            currentPositionInBar2 += b.Numerator % 2 == 0 ? b.Numerator : b.Numerator+1;
                        }

                        lastJ = j;
                        lastK = k;

                        if (currentPositionInBar1 < currentPositionInBar2)
                        {
                            j++;
                        }
                        else if (currentPositionInBar1 > currentPositionInBar2)

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
