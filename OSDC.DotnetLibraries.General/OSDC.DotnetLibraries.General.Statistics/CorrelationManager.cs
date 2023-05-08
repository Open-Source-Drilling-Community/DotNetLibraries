using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class CorrelationManager
    {
        private static CorrelationManager instance_ = null;
        private List<CorrelationGroup> groups_ = new List<CorrelationGroup>();
        private AccesManagement accesManagement_ = AccesManagement.Automatic;
        /// <summary>
        /// Singleton pattern
        /// </summary>
        public static CorrelationManager Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new CorrelationManager();
                }
                return instance_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AccesManagement AccesManagement
        {
            get { return accesManagement_; }
            set { accesManagement_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        public void Add(CorrelationGroup group)
        {
            if (group != null && !ContainsGroup(group))
            {
                group.AccesManagement = accesManagement_;
                groups_.Add(group);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool Remove(CorrelationGroup group)
        {
            group.Clear();
            return groups_.Remove(group);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            return Remove(GetGroup(name));
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CorrelationGroup> Groups
        {
            get { return groups_; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CorrelationGroup GetGroup(string name)
        {
            foreach (CorrelationGroup group in groups_)
            {
                if (group.Name == name)
                {
                    return group;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsDistribution(string name)
        {
            foreach (CorrelationGroup group in groups_)
            {
                if (group.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ContainsGroup(CorrelationGroup group)
        {
            if (groups_.Contains(group))
            {
                return true;
            }
            else
            {
                foreach (CorrelationGroup cg in groups_)
                {
                    if (cg.Name == group.Name)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetGroups()
        {
            foreach (CorrelationGroup group in groups_)
            {
                group.Reset();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            while (groups_.Count > 0)
            {
                Remove(groups_[0]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accesType"></param>
        public void SetManagement(AccesManagement accesType)
        {
            accesManagement_ = accesType;
            foreach (CorrelationGroup group in groups_)
            {
                group.AccesManagement = accesManagement_;
            }
        }

    }
}
