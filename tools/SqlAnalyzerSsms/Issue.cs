using Microsoft.VisualStudio.Text;

namespace SqlAnalyzerExtension
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Issue
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public SnapshotSpan SnapshotSpan { get; }

        public DiagnosticMessage Message { get; }

        public Issue(SnapshotSpan snapshotSpan, DiagnosticMessage message)
        {
            SnapshotSpan = snapshotSpan;
            Message = message;
        }

        public override bool Equals(object obj)
        {
            if (obj is Issue other)
            {
                return SnapshotSpan.Start.Position == other.SnapshotSpan.Start.Position && SnapshotSpan.End.Position == other.SnapshotSpan.End.Position && Message == other.Message;
            }

            return false;
        }
    }
}
