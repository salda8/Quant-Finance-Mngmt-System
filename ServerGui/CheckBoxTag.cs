using Common.EntityModels;

namespace ServerGui
{
    public class CheckBoxTag : CheckBoxItem<Tag>
    {
        public CheckBoxTag(Tag item, bool isChecked = false) : base(item, isChecked)
        {
        }
    }
}