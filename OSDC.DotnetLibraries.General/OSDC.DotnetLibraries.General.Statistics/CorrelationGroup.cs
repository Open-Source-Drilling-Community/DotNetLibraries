using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public enum CorrelationType
    {
        /// <summary>
        /// 
        /// </summary>
        Mixing,
        /// <summary>
        /// 
        /// </summary>
        Noise,
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public enum AccesManagement
    {
        /// <summary>
        /// 
        /// </summary>
        Automatic,
        /// <summary>
        /// 
        /// </summary>
        Manual,
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CorrelationGroup
    {
        private List<ContinuousDistribution> distributions_ = new List<ContinuousDistribution>();
        private double currentValue_ = Numeric.UNDEF_DOUBLE;
        private int requestNumber_ = 0;
        private string name_;
        private AccesManagement accesManagement_ = AccesManagement.Automatic;
        private string description_ = "";

        /// <summary>
        /// Default constructor
        /// </summary>
        public CorrelationGroup()
        {
        }

        /// <summary>
        /// Constructor with initialisation
        /// </summary>
        /// <param name="name"></param>
        public CorrelationGroup(string name)
        {
            name_ = name;
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
        public string Name
        {
            get { return name_; }
            set
            {
                name_ = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return description_; }
            set { description_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distribution"></param>
        public void Add(ContinuousDistribution distribution)
        {
            if (distribution != null && !Contains(distribution))
            {
                distributions_.Add(distribution);
                distribution.IsCorrelated = true;
                distribution.CorrelationGroup = this;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="distribution"></param>
        /// <param name="correlationCoefficient"></param>
        public void Add(ContinuousDistribution distribution, double correlationCoefficient)
        {
            if (distribution != null && !Contains(distribution))
            {
                distributions_.Add(distribution);
                distribution.IsCorrelated = true;
                distribution.CorrelationCoefficient = correlationCoefficient;
                distribution.CorrelationGroup = this;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            if (index >= 0 && index < distributions_.Count)
            {
                distributions_[index].IsCorrelated = false;
                distributions_[index].CorrelationGroup = null;
                distributions_.RemoveAt(index);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distribution"></param>
        public void Remove(ContinuousDistribution distribution)
        {
            if (distribution != null)
            {
                int index = -1;
                for (int i = 0; i < distributions_.Count; i++)
                {
                    if (distributions_[i].Equals(distribution))
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0 && index < distributions_.Count)
                {
                    Remove(index);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {

            int index = -1;
            for (int i = distributions_.Count - 1; i >= 0; i--)
            {
                if (distributions_[i].Name == name)
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0 && index < distributions_.Count)
            {
                Remove(index);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [XmlIgnore]
        public ContinuousDistribution this[int index]
        {
            get
            {
                if (distributions_ != null && index >= 0 && index < distributions_.Count)
                {
                    return distributions_[index];
                }
                else { return null; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Value
        {
            get
            {
                if (accesManagement_ == AccesManagement.Automatic)
                {
                    if (requestNumber_ % distributions_.Count == 0)
                    {
                        currentValue_ = RandomGenerator.Instance.NextDouble();
                        requestNumber_ = 1;
                    }
                    else
                    {
                        requestNumber_++;
                    }
                }
                else if (accesManagement_ == AccesManagement.Manual)
                {
                    if (requestNumber_ == 0)
                    {
                        currentValue_ = RandomGenerator.Instance.NextDouble();
                        requestNumber_ = 1;
                    }
                }

                return currentValue_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            if (accesManagement_ == AccesManagement.Manual)
            {
                requestNumber_ = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < distributions_.Count; i++)
            {
                distributions_[i].IsCorrelated = false;
                distributions_[i].CorrelationGroup = null;
            }
            distributions_.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get { return distributions_.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public List<ContinuousDistribution> Distributions
        {
            get { return distributions_; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            for (int i = 0; i < distributions_.Count; i++)
            {
                if (distributions_[i].Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distribution"></param>
        /// <returns></returns>
        public bool Contains(ContinuousDistribution distribution)
        {
            for (int i = 0; i < distributions_.Count; i++)
            {
                if (distributions_[i].Equals(distribution))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="name"></param>
        public void SetName(ContinuousDistribution dist, string name)
        {
            for (int i = 0; i < distributions_.Count; i++)
            {
                if (distributions_[i].Equals(dist))
                {
                    distributions_[i].Name = name;
                }
            }
        }
    }
}
