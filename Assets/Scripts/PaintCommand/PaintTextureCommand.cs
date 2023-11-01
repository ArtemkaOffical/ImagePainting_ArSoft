using UnityEngine;

namespace PaintCommand
{
    public class PaintTextureCommand : Command
    {
        private readonly PaintController _controller;
        private readonly Color32[] _currentColors32;
        private readonly Color32[] _previousColors32;

        public PaintTextureCommand(PaintController controller,Color32[] previousColors32, Color32[] currentColors32)
        {
            _controller = controller;
            _currentColors32 = currentColors32;
            _previousColors32 = previousColors32;
        }
        
        public override void Execute()
        {
            _controller.ApplyNewColors(_currentColors32);
        }

        public override void Undo()
        {
            _controller.ApplyNewColors(_previousColors32);
        }
    }
}