using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class PredicateSetProcessor
    {
        private readonly Smells smells;

        public PredicateSetProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessPredicateSetStatement(PredicateSetStatement fragment)
        {
            switch (fragment.Options)
            {
                case SetOptions.AnsiNulls:
                    if (!fragment.IsOn)
                    {
                        smells.SendFeedBack(14, fragment);
                    }

                    return;
                case SetOptions.AnsiPadding:
                    if (!fragment.IsOn)
                    {
                        smells.SendFeedBack(15, fragment);
                    }

                    return;
                case SetOptions.AnsiWarnings:
                    if (!fragment.IsOn)
                    {
                        smells.SendFeedBack(16, fragment);
                    }

                    return;
                case SetOptions.ArithAbort:
                    if (!fragment.IsOn)
                    {
                        smells.SendFeedBack(17, fragment);
                    }

                    return;
                case SetOptions.NumericRoundAbort:
                    if (fragment.IsOn)
                    {
                        smells.SendFeedBack(18, fragment);
                    }

                    return;
                case SetOptions.QuotedIdentifier:
                    if (!fragment.IsOn)
                    {
                        smells.SendFeedBack(19, fragment);
                    }

                    return;
                case SetOptions.ForcePlan:
                    if (fragment.IsOn)
                    {
                        smells.SendFeedBack(20, fragment);
                    }

                    return;
                case SetOptions.ConcatNullYieldsNull:
                    if (!fragment.IsOn)
                    {
                        smells.SendFeedBack(13, fragment);
                    }

                    return;
                case SetOptions.NoCount:
                    if (fragment.IsOn)
                    {
                        smells.ProcedureStatementBodyProcessor.NoCountSet = true;
                    }

                    return;
            }
        }
    }
}