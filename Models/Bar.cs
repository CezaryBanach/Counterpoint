using Melanchall.DryWetMidi.Smf.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.Models
{
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
}
