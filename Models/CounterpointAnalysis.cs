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
        public List<string> CounterpointCommentsLog;
        Mode Mode;
        Tonic Tonic;
        int CantusFirmusId;

        /// <summary>
        /// contains allowed intervals(Distance between note and tonic)
        /// </summary>
        List<int> Scale;
        
        private List<int> GetScale(Mode mode, Tonic tonic)
        {
            //Ionian mode
            List<int> sequence = new List<int> {2,2,1,2,2,2,1};
            for(int i = 0; i< (int)mode; i++)
            {
                sequence.Add(sequence[0]);
                sequence.RemoveAt(0);
            }

            for(int i = 1; i< sequence.Count; i++)
            {
                sequence[i] += sequence[i - 1];
            }
            return sequence;
        }
        

        public CounterpointAnalysis(string path, CounterpointSpecie selectedSpecie, Mode mode, Tonic tonic, int cantusFirmusId)
        {           
            MidiFile file = MidiFile.Read(path);
            Piece = new Piece(file);
            SelectedSpecie = selectedSpecie;
            CounterpointCommentsLog = new List<string>();
            Mode = mode;
            Tonic = tonic;
            Scale = GetScale(mode, tonic);
            CantusFirmusId = cantusFirmusId;

            if(CantusFirmusId >= Piece.VoiceLines.Count)
            {
                CounterpointCommentsLog.Add("Detected number of voices: " + Piece.VoiceLines.Count.ToString() + "."
                    + " Selected Cantus Firmus with smaller voice number.");
                return;
            }
        
            foreach (var voiceline in Piece.VoiceLines)
            {
                voiceline.PopulateHorizontalIntervals();
            }            
            Piece.PopulateVerticalIntervals();

            //Logi do debugowania
            log.WriteLog(SelectedSpecie.ToUserFriendlyString());
            
            foreach (var a in Piece.VoiceLines[CantusFirmusId].VerticalIntervals)
            {
                log.WriteLog("VoiceVerticalitnerval");
                int i = 0;
                foreach(var b in a)
                {
                    log.WriteLog((b%12).ToString());
                    log.WriteLog(Piece.VoiceLines[CantusFirmusId].NotesUngrouped[i].ToString());
                    log.WriteLog(Piece.VoiceLines[0].NotesUngrouped[i].ToString());
                    log.WriteLog(GetNamedInterval((b % 12)).ToString());

                    if (i+1< Piece.VoiceLines[CantusFirmusId].NotesUngrouped.Count())
                    {
                        i++;
                    }
                   
                }
            }

            RunCounterpointAnalysis(Piece, SelectedSpecie);
            
           
        } 
        private void RunCounterpointAnalysis(Piece piece, CounterpointSpecie specie)
        {

            //na razie zakladamy ze wszystko to 3 specie xddd
                var cantusFirmus = piece.VoiceLines[CantusFirmusId];
                
                for (int i = 0; i < piece.VoiceLines.Count && piece.VoiceLines[i].VoiceId != cantusFirmus.VoiceId; i++)
                {
                    VoiceLine voiceLine = piece.VoiceLines[i];
                    CheckExposedTritones(voiceLine, specie);
                   // CheckExposedFifths(voiceLine, specie);

                    //check forbidden skips:
                    // aug fourth, seventh, octave+, descending minor sixths, major sixths.

                    //aug fourth
                    CheckSkip(voiceLine, specie, 6, 1, 1);
                    CheckSkip(voiceLine, specie, 6, -1, 1);

                    // seventh
                    CheckSkip(voiceLine, specie, 10, 1, 1);
                    CheckSkip(voiceLine, specie, 10, -1, 1);

                    CheckSkip(voiceLine, specie, 11, 1, 1);
                    CheckSkip(voiceLine, specie, 11, -1, 1);

                    //sixth
                    CheckSkip(voiceLine, specie, 9, 1, 1);
                    CheckSkip(voiceLine, specie, 9, -1, 1);
                    CheckSkip(voiceLine, specie, 8, -1, 1);

                    //Leaps bigger than octave
                    CheckSkipInequality(voiceLine, specie, 12, 1, 1, 1);
                    CheckSkipInequality(voiceLine, specie, 12, -1, 1, 1);

                    //check for repeated notes
                    CheckSkip(voiceLine, specie, 0, 1, 1, ". Repeated notes are not strictly forbidden, but should be avoided.");

                    //Tcheck for 2+ skips in the same direction
                    CheckForStepsInSameDirection(voiceLine, specie, 3);

                    CheckCounterpointMode(cantusFirmus, voiceLine, specie);

                    CheckForNotesOutOfMode(cantusFirmus, voiceLine, specie);

                    //check closing formula
                    //allowed for upper cf:3m-5-4-3m-1 
                    //allowed for upper cf:5-3m-1
                    //allowed for lower cf:8-7-5-6w-8
                    //allowed for lower cf:3-4-5-6w-8
                    CheckClosingFormula(cantusFirmus, voiceLine,specie);

                    // check perfect consonance(unison, fifth, octave) entrance
                    CheckPerfectConsonanceEntrance(cantusFirmus, voiceLine, specie);

                    //TODO check dissonnance (allowed in 2+ species)
                    CheckForDissonnance(cantusFirmus, voiceLine, specie);

                    //CheckForNotesOutOfMode(cantusFirmus,voiceLine,specie);

                    //TODO check for parallel fifths/octaves

                    //TODO check for cambiata
                }                       
        }

        private void CheckForDissonnance(VoiceLine cantusFirmus,VoiceLine voiceLine, CounterpointSpecie specie)
        {
            var intervals = cantusFirmus.VerticalIntervals[voiceLine.VoiceId];

            for (int i = 0; i < intervals.Count - 1; i++)
            {
                var interval = intervals[i];
                var firstNote = cantusFirmus.NotesUngrouped[0];

                var intervalFromFirstNote = (GetIntervalInSemitones(firstNote, voiceLine.NotesUngrouped[i]));

                if (intervalFromFirstNote % 12 == 0)
                    continue;

                if (Scale.Contains(intervalFromFirstNote%12) )
                {
                    int noteIndex = Scale.IndexOf(intervalFromFirstNote % 12);
                    if(!IsConsonance(interval))
                    {
                        if ((IsConsonance(intervals[i - 1]) && IsConsonance(intervals[i + 1]))
                          && (Math.Abs(voiceLine.NotesUngrouped[i - 1].NoteNumber - firstNote.NoteNumber) % 12
                            == Scale[(noteIndex - 1)>0? (noteIndex - 1) % 7: 0]
                          && Math.Abs(voiceLine.NotesUngrouped[i + 1].NoteNumber - firstNote.NoteNumber) % 12
                            == Scale[(noteIndex + 1) % 7])
                          || (Math.Abs(voiceLine.NotesUngrouped[i - 1].NoteNumber - firstNote.NoteNumber) % 12
                            == Scale[(noteIndex + 1) % 7]
                          && Math.Abs(voiceLine.NotesUngrouped[i + 1].NoteNumber - firstNote.NoteNumber) % 12
                            == Scale[(noteIndex - 1) > 0 ? (noteIndex - 1) % 7 : 0]))
                        {

                        }
                      else
                        {
                            CounterpointCommentsLog.Add("Improperly resolved disonance at note " + (i+1).ToString() + ".");
                        }
               
                    }
                }
                else
                {
                    CounterpointCommentsLog.Add("Note " + (i +1).ToString() + " is out of mode!");
                }
          
            }
        }
        private bool IsConsonance(int intervalInSemitones)
        {
            var interval = intervalInSemitones % 12;
            if (interval == 1 ||
                interval == 2 ||
                interval == 5 ||
                interval == 6 ||
                interval == 10 ||
                interval == 11)
            {
                return false;
            }
            else
            {
                return true;
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
                if(cantusFirmus.VerticalIntervals[voiceLine.VoiceId].Count>0)
                {
                      if (cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0]%13 == 12 ||
                       cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] % 13 == 0 ||
                       cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0] % 13 == 7)
                      {

                      }
                      else
                      {
                          CounterpointCommentsLog
                              .Add("Mode has been changed- first interval should be octave, fifth or unison."
                              + "Current interval: " + cantusFirmus.VerticalIntervals[voiceLine.VoiceId][0].ToString());
                      }
                }

            }

        }
        private void CheckExposedTritones(VoiceLine cantusFirmus,CounterpointSpecie specie)
        {
            CheckSkip(cantusFirmus,specie, 6, 1, 2,  " (Exposed tritone)");
            CheckSkip(cantusFirmus,specie, 6, -1, 2, " (Exposed tritone)");

        }
        private void CheckExposedFifths(VoiceLine cantusFirmus, CounterpointSpecie specie)
        {
            CheckSkip(cantusFirmus,specie, 7, 1, 2,  " (Exposed fifth)" );
            CheckSkip(cantusFirmus,specie, 7, -1, 2, " (Exposed fifth)" );

        }
        private void CheckPerfectConsonanceEntrance(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie)
        {            
            for (int i = 0;i>0 && i + 1 < cantusFirmus.NotesUngrouped.Count; i++)
            {
                var interval = cantusFirmus.VerticalIntervals[voiceLine.VoiceId][i];
                if ( interval == 0 || interval == 7 || interval == 12)
                {
                    if(cantusFirmus.HorizontalIntervals[i-1]* voiceLine.HorizontalIntervals[i-1] <= 0)
                    {
                       
                    }
                    else
                    {
                        CounterpointCommentsLog.Add("You entered a perfect consonance (a " + GetNamedInterval(interval)
                                                    + ") by means of direct motion at note " + (i +1) + ".");
                    }
                }                                         
            }
        }


        private void CheckForNotesOutOfMode(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie)
        {
            int i = 1;
            foreach(var interval in cantusFirmus.VerticalIntervals[voiceLine.VoiceId])
            {
               int index = Scale.BinarySearch(interval);
                
                if (index>=0 &&Scale[index] != interval)
                    CounterpointCommentsLog.Add("Note " + (i).ToString() + " is out of selected scale");

                i++;
            }
       
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
        private void CheckSkip(VoiceLine voiceLine, CounterpointSpecie specie,
                                     int interval, int skipDirection, int NoteDistance, string optionalErrorComment ="")
        {
            for (int i = 0; Math.Max(i,i + NoteDistance)< voiceLine.NotesUngrouped.Count; i++)
            {

                if (skipDirection * (voiceLine.NotesUngrouped[i].NoteNumber - voiceLine.NotesUngrouped[i + NoteDistance].NoteNumber) 
                    == interval)
                {
                    CounterpointCommentsLog.Add( "Forbidden interval in voice "+ (voiceLine.VoiceId+1).ToString() +" ("
                                                + GetNamedInterval(interval)
                                                + ") between notes "
                                                + (i+1).ToString() +" (" +voiceLine.NotesUngrouped[i].ToString() +"), " 
                                                + (i + 1 + NoteDistance).ToString()
                                                + " (" + voiceLine.NotesUngrouped[i+ NoteDistance].ToString() + ")"
                                                + optionalErrorComment);                                               
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
        private void CheckSkipInequality(VoiceLine voiceLine, CounterpointSpecie specie,
                             int interval, int skipDirection, int NoteDistance, int inequalityType)
        {
            for (int i = 0; Math.Max(i, i + NoteDistance) < voiceLine.NotesUngrouped.Count; i++)
            {
                if (inequalityType * skipDirection * (voiceLine.NotesUngrouped[i].NoteNumber - voiceLine.NotesUngrouped[i + NoteDistance].NoteNumber)
                    > inequalityType * interval)
                {
                    CounterpointCommentsLog.Add("Forbidden interval ("
                                                + GetNamedInterval(interval)
                                                + ") between notes "
                                                + i.ToString() + ", " + (i + NoteDistance).ToString());
                }
            }
        }

        private void CheckForStepsInSameDirection(VoiceLine voiceLine, CounterpointSpecie specie, int numberOfSkips)
        {
            int currentNumberOfSteps = 1;

            for (int i = 0; (i + 1)< voiceLine.NotesUngrouped.Count-1; i++)
            {

                if(voiceLine.HorizontalIntervals[i]* voiceLine.HorizontalIntervals[i+1]>0)
                {
                    currentNumberOfSteps++;
                    if(currentNumberOfSteps >= numberOfSkips)
                    {
                        CounterpointCommentsLog.Add("Warning: " + currentNumberOfSteps + " steps in the same direction starting at note " 
                            + i.ToString() + ".");
                    }
                }
                else
                {
                    currentNumberOfSteps = 1;
                }
            }
        }

        private void CheckClosingFormula(VoiceLine cantusFirmus,VoiceLine voiceLine,CounterpointSpecie specie)
        {
            List<List<int>> allowedClosingFormulas = AllowedClosingFormula(cantusFirmus, voiceLine, specie);

            foreach (var allowedClosingFormula in allowedClosingFormulas)
            {
                int i = cantusFirmus.NotesUngrouped.Count - 2;
                int k = allowedClosingFormula.Count - 1;

                while (k >= 0 && i >= 0)
                {
                    if (cantusFirmus.HorizontalIntervals[i] == Math.Abs(allowedClosingFormula[k]))
                    {
                        k--;
                        i--;
                    }
                    else
                    {
                        return;
                    }
                }
                if (k > 0)
                {
                    CounterpointCommentsLog.Add("Invalid closing formula. Proper closing formula consists of:");
                    string proper = "";
                    foreach(var a in allowedClosingFormula)
                    {
                        proper += GetNamedInterval(a) + ", ";
                    }
                    proper = proper.Substring(0, proper.Length - 2);
                    proper += ".";

                }
            }

        }
        private List<List<int>> AllowedClosingFormula(VoiceLine cantusFirmus, VoiceLine voiceLine, CounterpointSpecie specie)
        {
           bool CantusFirmusIsHigherVoice = false;
           if (cantusFirmus.VerticalIntervals[voiceLine.VoiceId].Any(x => x < 0))
            {
                CantusFirmusIsHigherVoice = true;
            }
           else
            {
                CantusFirmusIsHigherVoice = false;
            }

           if(CantusFirmusIsHigherVoice)
            {
                switch ((int)specie)
                {
                    case 1:
                        return new List<List<int>> { new List<int> { 3, 0 } };
                    case 2:
                        return new List<List<int>> { new List<int> { 7, 3, 0 } };
                    case 3:
                        return new List<List<int>> { new List<int> { 3, 7, 5, 3, 0 } };
                    case 4:
                        return new List<List<int>> { new List<int> { 2, 3, 0 } };
                    case 5:
                        return new List<List<int>> { new List<int> { 2, 3, 0 } };
                    default:
                        return null;
                }
            }
           else
            {
                switch ((int)specie)
                {
                    case 1:
                        return new List<List<int>> { new List<int> { 9, 12 } };
                    case 2:
                        return new List<List<int>> { new List<int> { 7, 9, 12 } };
                    case 3:
                        return new List<List<int>> { new List<int> { 3, 5, 7, 9, 12 }, new List<int> { 12, 10, 7, 9, 12} };
                    case 4:
                        return new List<List<int>> { new List<int> { 2, 3, 0 } };
                    case 5:
                        return new List<List<int>> { new List<int> { 2, 3, 0 } };
                    default:
                        return null;
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
 
}
