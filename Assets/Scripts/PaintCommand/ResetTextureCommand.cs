using UnityEngine;

namespace PaintCommand
{
    public class ResetTextureCommand : Command
    {
        private readonly PaintController _controller;
        private readonly Color32[] _currentColors32;

        public ResetTextureCommand(PaintController controller, Color32[] currentColors32)
        {
            _controller = controller;
            _currentColors32 = currentColors32;
        }
        public override void Execute()
        {
           _controller.ResetTexture();
        }

        public override void Undo()
        {
            _controller.ApplyNewColors(_currentColors32);
        }
    }
}