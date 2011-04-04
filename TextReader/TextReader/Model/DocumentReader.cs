using System;
using System.Collections.Generic;
using System.Text;
using System.Speech.Synthesis;
using System.Windows.Documents;
using System.ComponentModel;

namespace TextReader
{
    /// <summary>
    /// The class the controls all the reading from a TextPointer, and forward.
    /// </summary>
    class DocumentReader : INotifyPropertyChanged
    {

        /// <summary>
        /// The SpeechSynthesizer that does all the hard work with reading up the text.
        /// </summary>
        private SpeechSynthesizer _synth;

        /// <summary>
        /// Indicates where the synthesizer was started to read
        /// </summary>
        private TextPointer startedReading;
        /// <summary>
        /// Indicates where the synthesizer was set to stop reading. Where it will start reading from again when it stops.
        /// </summary>
        private TextPointer breakPoint;
        /// <summary>
        /// Set when the user asks to stop reading, so it doesn't start rading from breakPoint again.
        /// And set to false when the text first starts reading.
        /// </summary>
        private bool stopReading;
        /// <summary>
        /// Used with countToLastPoint in SpeakProgress to save where it came to, so it doesn't have to recalculate the
        /// position with GetPositionAtTextOffset. Also used by change voice/rate to know where to continue to speak from.
        /// </summary>
        private TextPointer lastPoint;
        /// <summary>
        /// Used with lastPoint in SpeakProgress to save where it came to, so it doesn't have to recalculate the
        /// position with GetPositionAtTextOffset.
        /// </summary>
        private int countToLastPoint;
        

        public DocumentReader()
        {
            State = ReaderState.NotSpeaking;
            stopReading = true;
            _synth = new SpeechSynthesizer();
            _synth.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(_synth_SpeakProgress);
            _synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(_synth_SpeakCompleted);
            _synth.SpeakStarted += new EventHandler<SpeakStartedEventArgs>(_synth_SpeakStarted);
        }

        void _synth_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            stopReading = false;
            State = ReaderState.Speaking;
        }

        void _synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            if (stopReading | breakPoint.CompareTo(breakPoint.DocumentEnd) == 0)
            {
                State = ReaderState.NotSpeaking;
            }
            else
            {
                StartReading(breakPoint);
            }
        }

        // this will be called each time a new word is begining to be read by the SpeechSynthesizer
        void _synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            TextPointer newPos;

            // Get a TextPointer for the start of the word
            if (countToLastPoint < e.CharacterPosition)
            {
                // use he lsatPoint if posible
                newPos = GetPositionAtTextOffset(lastPoint, e.CharacterPosition - countToLastPoint);
            }
            else
            {
                // otherwise start from the start
                newPos = GetPositionAtTextOffset(startedReading, e.CharacterPosition);
            }


            if (newPos == null)
            {
                ReadText = null;
                return;
            }
            
            // And get the end
            var end = GetPositionAtTextOffset(newPos, e.Text.Length);

            if (end == null)
            {
                end = newPos;
            }
            else
            {
                // update lastpoint
                lastPoint = end;
                countToLastPoint = e.CharacterPosition + e.Text.Length;
            }

            // Create a new TextRange that indicates the read word
            // a new TextRange is created to make sure protychanged is called
            // and noone else use the old textrange.
            ReadText = new TextRange(newPos, end);
        }

        private static TextPointer GetPositionAtTextOffset(TextPointer source, int count)
        {

            // this function might be slow if we have to travers across meny blocks,
            // might be an idea to save positions and counts so you can later check them first
            // so you don't have to recalculate large numbers
           
            var res = source;

            TextPointer lastPoint = null;

            // we simly travers the textpoint untill we count reaches 0
            while (res != null && count > 0)
            {
                if (lastPoint != null && lastPoint.CompareTo(res) == 0)
                    break;
                lastPoint = res;
                int length = res.GetTextRunLength(LogicalDirection.Forward);
                // If it's within the run we just jump to the position
                if (count <= length)
                {
                    res = res.GetPositionAtOffset(count);
                    break;
                }
                else
                {
                    // Otherwise we jump across the run and get to the next position
                    count -= length;
                    res = res.GetPositionAtOffset(length);
                    res = res.GetInsertionPosition(LogicalDirection.Forward);
                    // To check if we exit an paragraph
                    var lastPara = res.Paragraph;
                    if (res.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
                    {
                        res = res.GetPositionAtOffset(2, LogicalDirection.Forward);
                        // if we enter a new paragraph, count up by two
                        // res might be null if we reach the end of the document
                        if (res != null && lastPara != res.Paragraph)
                            count -= 2;
                    }
                }
            }
            // Make sure we return either null or a InsertionPosition
            if (res != null && !res.IsAtInsertionPosition)
            {
                res = res.GetNextInsertionPosition(LogicalDirection.Forward);
            }
            return res;
        }

        public IEnumerable<String> GetVoices()
        {
            foreach (var voice in _synth.GetInstalledVoices())
            {
                yield return voice.VoiceInfo.Name;
            }
        }

        /// <summary>
        /// If it is reading stops reading, calls setter, and start reading again.
        /// If it isn't reading just runs setter inside try.
        /// Finaly it calls OnPropertyChanged if the value from getter doesn't match,
        /// and returns the new value from getter.
        /// </summary>
        /// <typeparam name="t">Type of the value to set.</typeparam>
        /// <param name="getter">A function that returns the value.</param>
        /// <param name="setter">A function that sets the value.</param>
        /// <param name="matcher">A function that can check if the value has changed.</param>
        /// <param name="property">The name of the property that is changed.</param>
        /// <returns>The from getter after setter have been called.</returns>
        private t setProperty<t>(Func<t> getter, Action setter, Func<t,t,bool> matcher,String property)
        {
            var oldValue = getter.Invoke();
            if (stopReading)
            {
                try
                {
                    setter.Invoke();
                }
                catch { }
            }
            else
            {
                try
                {
                    StopReading();

                    setter.Invoke();
                }
                finally
                {
                    StartReading(lastPoint);
                }
            }
            var newValue = getter.Invoke();
            if (!matcher.Invoke(oldValue,newValue))
            {
                OnPropertyChanged(property);
            }
            return newValue;
        }

        /// <summary>
        /// The voice that is now speaking.
        /// </summary>
        /// <returns>The voice that is now speaking. This should be one of the strings from GetVoices.</returns>
        public string GetVoice()
        {

           return _synth.Voice.Name;
        }

        /// <summary>
        /// Sets the voice that should be used to speak.
        /// </summary>
        /// <param name="voice">One of the strings from GetVoices.</param>
        /// <returns>
        /// The voice that is now speaking, this might not be the
        /// same as the selected one, if an error occoured.
        /// </returns>
        public string SetVoice(String voice)
        {
            return setProperty(GetVoice, () => { _synth.SelectVoice(voice); }, (a, b) => a == b, "Voice");
        }

        /// <summary>
        /// Gets the volume at which it speaks.
        /// </summary>
        /// <returns>The volume at which it speaks, a number in [0;100].</returns>
        public int GetVolume()
        {
            return _synth.Volume;
        }

        /// <summary>
        /// Sets the volume that should be used to speak.
        /// </summary>
        /// <param name="volume">The volume at which it speaks, a number in [0;100].</param>
        /// <returns>
        /// The volume as it is now, this might not be the
        /// same as the selected one, if an error occoured.
        /// </returns>
        public int SetVolume(int volume)
        {
            return setProperty(GetVolume, () => { _synth.Volume = volume; }, (a, b) => a == b, "Volume");
        }

        /// <summary>
        /// Gets the rate at which it speaks.
        /// </summary>
        /// <returns>The rate at which it speaks, a number in [-10;10].</returns>
        public int GetRate()
        {
            return _synth.Rate;
        }

        /// <summary>
        /// Sets the rate at which it speaks.
        /// </summary>
        /// <param name="rate">The rate at which it speaks, must be between in [-10;10].</param>
        /// <returns>
        /// The rate as it is now, this might not be the
        /// same as the selected one, if an error occoured.
        /// </returns>
        public int SetRate(int rate)
        {
            return setProperty(GetRate, () => { _synth.Rate = rate; }, (a, b) => a == b, "Rate");
        }

        public void StartReading(TextPointer startingPoint)
        {
            if (startingPoint == null)
                return;
            StopReading();

            startedReading = startingPoint;
            lastPoint = startedReading;
            countToLastPoint = 0;
            breakPoint = GetPositionAtTextOffset(startedReading, 1000);
            if (breakPoint == null)
                breakPoint = startedReading.DocumentEnd;
            if (breakPoint.Paragraph != null)
                breakPoint = breakPoint.Paragraph.ContentEnd;

            _synth.SpeakAsync(richTextBoxToPromt(startingPoint,breakPoint));
            ReadText = new TextRange(startingPoint, startingPoint);
            if (_synth.State == SynthesizerState.Paused)
            {
                _synth.Resume();
            }
            State = ReaderState.StartedSpeaking;
        }
        public void StopReading()
        {
            stopReading = true;
            _synth.SpeakAsyncCancelAll();
        }

        public void PauseReading()
        {
            if (_synth.State == SynthesizerState.Speaking)
            {
                _synth.Pause();
                State = ReaderState.Paused;
            }
        }
        public void ResumeReading()
        {
            if (_synth.State==SynthesizerState.Paused )
            {
                _synth.Resume();
                State = ReaderState.Speaking;
            }
        }

        private String richTextBoxToPromt(TextPointer startingPoint, TextPointer endingPoint)
        {
            var res = new StringBuilder();


            TextRange text = new TextRange(startingPoint, endingPoint);
            res.Append(text.Text);
            
            return res.ToString();
        }

        public ReaderState State
        {
            get { return _state; }
            private set{
                _state = value;
                OnPropertyChanged("State");
            }
        }

        public TextRange ReadText
        {
            get { return _readText; }
            protected set
            {
                _readText = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ReadText");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this,new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private TextRange _readText;
        private ReaderState _state;
    }

    enum ReaderState
    {
        StartedSpeaking,
        Speaking,
        Paused,
        NotSpeaking
    }
}
