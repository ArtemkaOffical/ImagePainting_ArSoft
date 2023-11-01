using System.Collections.Generic;

namespace PaintCommand
{
   
    public class CommandInvoker
    {
        
       private Stack<Command> _commands{ get; set; } = new Stack<Command>{};

       private Command _command { get; set; }
        
        public void Execute(Command command)
        {
            _command = command; 
            _command.Execute();
            _commands.Push(_command);
        }

        public void Undo()
        {
            if (_commands.Count == 0)
                return;
            
            _command = _commands.Pop();
            _command.Undo();
        }

    }
}