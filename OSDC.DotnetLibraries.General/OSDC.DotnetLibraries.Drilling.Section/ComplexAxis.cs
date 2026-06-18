using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OSDC.DotnetLibraries.Drilling.Section
{
    /// <summary>
    /// A class that represents a 3D axis, i.e., composed of CircularArcSection that are defined by XYZ coordinates but which can have additionally incl and azimuth constraints
    /// </summary>
    public class ComplexAxis : IList<CircularArcSection>
    {
        private List<CircularArcSection> sections_ = new List<CircularArcSection>();

        /// <summary>
        ///  calculate the complex axis
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            bool check = true;
            for (int i = 1; i < sections_.Count; i++)
            {
                if (sections_[i].Start != sections_[i - 1].End)
                {
                    check = false;
                    break;
                }
            }
            return check;
        }
        /// <summary>
        /// index accessor. It makes sure that the start of the section is actually the end of the previous section (if any).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CircularArcSection this[int index] { 
            get => sections_[index]; 
            set
            {
                sections_[index] = value;
                if (index > 0)
                {
                    sections_[index].Start = sections_[index - 1].End;
                }
            }
        }

        public int Count => sections_.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// ensure to link start to the end of the previous section
        /// </summary>
        /// <param name="item"></param>
        public void Add(CircularArcSection item)
        {
            sections_.Add(item);
            if (sections_.Count > 1)
            {
                item.Start = sections_[sections_.Count - 2].End;
            }
        }

        public void Clear()
        {
            sections_.Clear();
        }

        public bool Contains(CircularArcSection item)
        {
            return sections_.Contains(item);
        }

        public void CopyTo(CircularArcSection[] array, int arrayIndex)
        {
            sections_.CopyTo(array, arrayIndex);
        }

        public IEnumerator<CircularArcSection> GetEnumerator()
        {
            return sections_.GetEnumerator();
        }

        public int IndexOf(CircularArcSection item)
        {
            return sections_.IndexOf(item);
        }

        /// <summary>
        /// ensure that the start points to the end of previous section and the next section start points to the end of item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, CircularArcSection item)
        {
            sections_.Insert(index, item);
            if (sections_.Count > 1)
            {
                item.Start = sections_[index - 1].End;
            }
            if (index < sections_.Count)
            {
                sections_[index + 1].Start = item.End;
            }
        }

        public bool Remove(CircularArcSection item)
        {
            return sections_.Remove(item);
        }

        public void RemoveAt(int index)
        {
            sections_.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
