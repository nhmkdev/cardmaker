using System.Drawing;
using CardMaker.XML;

namespace CardMaker.Data
{
    public class ElementPosition
    {
        public ElementPosition(ProjectLayoutElement zElement)
        {
            BoundsRectangle = new Rectangle(zElement.x, zElement.y, zElement.width, zElement.height);
            Rotation = zElement.rotation;
        }

        public Rectangle BoundsRectangle { get; set; }
        public float Rotation { get; set; }
    }
}
