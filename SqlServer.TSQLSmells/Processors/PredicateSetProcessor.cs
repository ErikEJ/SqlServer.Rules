using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class PredicateSetProcessor
    {
        private readonly Smells _smells;

        public PredicateSetProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessPredicateSetStatement(PredicateSetStatement Fragment)
        {
            switch (Fragment.Options)
            {
                case SetOptions.AnsiNulls:
                    if (!Fragment.IsOn) _smells.SendFeedBack(14, Fragment);
                    return;
                case SetOptions.AnsiPadding:
                    if (!Fragment.IsOn) _smells.SendFeedBack(15, Fragment);
                    return;
                case SetOptions.AnsiWarnings:
                    if (!Fragment.IsOn) _smells.SendFeedBack(16, Fragment);
                    return;
                case SetOptions.ArithAbort:
                    if (!Fragment.IsOn) _smells.SendFeedBack(17, Fragment);
                    return;
                case SetOptions.NumericRoundAbort:
                    if (Fragment.IsOn) _smells.SendFeedBack(18, Fragment);
                    return;
                case SetOptions.QuotedIdentifier:
                    if (!Fragment.IsOn) _smells.SendFeedBack(19, Fragment);
                    return;
                case SetOptions.ForcePlan:
                    if (Fragment.IsOn) _smells.SendFeedBack(20, Fragment);
                    return;
                case SetOptions.ConcatNullYieldsNull:
                    if (!Fragment.IsOn) _smells.SendFeedBack(13, Fragment);
                    return;
                case SetOptions.NoCount:
                    if (Fragment.IsOn) _smells.ProcedureStatementBodyProcessor.NoCountSet = true;
                    return;
            }
        }
    }
}