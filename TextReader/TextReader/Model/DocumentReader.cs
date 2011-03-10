using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Speech.Synthesis;
using System.Windows.Documents;
using System.Windows;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace TextReader
{
    /// <summary>
    /// The class the controls all the reading from a selected RichTextBox
    /// </summary>
    class DocumentReader : INotifyPropertyChanged
    {

        private SpeechSynthesizer _synth;

        public DocumentReader()
        {
            State = ReaderState.NotSpeaking;
            _synth = new SpeechSynthesizer();
            _synth.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(_synth_SpeakProgress);
            _synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(_synth_SpeakCompleted);
            _synth.SpeakStarted += new EventHandler<SpeakStartedEventArgs>(_synth_SpeakStarted);
        }

        void _synth_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            State = ReaderState.Speaking;
        }

        void _synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            State = ReaderState.NotSpeaking;
        }

        void _synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            // undo colors
          //  unindicateWord();

            var newPos = GetPositionAtTextOffset(startedReading,e.CharacterPosition);

            if (newPos == null)
            {
                ReadText = null;
            }
            //    return;
            
            // indicate the word read
            var end =GetPositionAtTextOffset( newPos,e.Text.Length);
            if (end == null)
                end = newPos;

            ReadText = new TextRange(newPos, end);
        }

        static TextPointer GetPositionAtTextOffset(TextPointer source, int count)
        {
            var res = source;
            var lastPara = res.Paragraph;


            TextPointer lastPoint = null;
            while (res != null && count > 0)
            {
                if (lastPoint != null && lastPoint.CompareTo(res) == 0)
                    break;
                lastPoint = res;
                int length = res.GetTextRunLength(LogicalDirection.Forward);
                if (count <= length)
                {
                    res = res.GetPositionAtOffset(count);
                    break;
                }
                else
                {
                    count -= length;
                    res = res.GetPositionAtOffset(length);
                    res = res.GetInsertionPosition(LogicalDirection.Forward);
                    if (res.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
                    {
                        res = res.GetPositionAtOffset(2);
                    }
                    if (res != null && lastPara != res.Paragraph)
                    {
                        // counter the countdown that happens if we're stood just outside a paragraph
                        if (lastPara == null)
                        {
                            count += 2;
                        }
                        lastPara = res.Paragraph;
                        count -= 2;
                    }
                }
            }
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
            //return _synth.GetInstalledVoices().SelectMany(s => s.VoiceInfo.Name );
        }

        public string GetVoice()
        {

            return _synth.Voice.Name;
        }
        public string SetVoice(String voice)
        {
            try
            {
                _synth.SpeakAsyncCancelAll();
                _synth.SelectVoice(voice);

                OnPropertyChanged("Voice");
            }
            catch (Exception)
            {
            
            }
            return _synth.Voice.Name;
        }

        public void SetRate(int value)
        {
            _synth.Rate = value;
            OnPropertyChanged("Rate");
        }
        public int GetRate()
        {
            return _synth.Rate;
        }

        public void StartReading(TextPointer startingPoint)
        {
            _synth.SpeakAsyncCancelAll();
            _synth.SpeakAsync(richTextBoxToPromt(startingPoint));
            if (_synth.State == SynthesizerState.Paused)
            {
                _synth.Resume();
            }
            startedReading = startingPoint;
            State = ReaderState.StartedSpeaking;
        }
        public void StopReading()
        {
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

        TextPointer startedReading;
        private String richTextBoxToPromt(TextPointer startingPoint)
        {
            var res = new StringBuilder();
        
            TextRange text = new TextRange(startingPoint, startingPoint.DocumentEnd);
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
