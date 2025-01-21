using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class SetProcessor
    {
        private readonly Smells smells;

        public SetProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void ProcessGeneralSetCommand(GeneralSetCommand setCommand)
        {
            switch (setCommand.CommandType)
            {
                case GeneralSetCommandType.DateFirst:
                    smells.SendFeedBack(9, setCommand);
                    break;
                case GeneralSetCommandType.DateFormat:
                    smells.SendFeedBack(8, setCommand);
                    break;
            }
        }

        public void ProcessSetStatement(SetCommandStatement fragment)
        {
            foreach (GeneralSetCommand setCommand in fragment.Commands)
            {
                ProcessGeneralSetCommand(setCommand);
            }
        }
    }
}