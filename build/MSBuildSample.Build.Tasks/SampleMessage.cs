using Microsoft.Build.Framework;
using System;

namespace MSBuildSample.Build.Tasks
{
    public class SampleMessage
        : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        private MessageImportance importance = MessageImportance.Normal;
        public string Importance {
            get => importance.ToString();
            set => importance = (MessageImportance)Enum.Parse(typeof(MessageImportance), value, true);
        }

        [Required]
        public string Text { get; set; }

        public bool Execute()
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(Text, null, null, importance));
            return true;
        }
    }
}
