using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    /// <summary>
    /// Discrete distribution defined by its sequence of probabilities.
    /// The sequence (p_i)_{i\in \N} means Prob[X=i] = p_i.
    /// </summary>
    public class GeneralDiscreteDistribution : DiscreteDistribution
    {
        private List<double>? probabilities_ = new List<double>();

        /// <summary>
        /// 
        /// </summary>
        public GeneralDiscreteDistribution()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="probas"></param>
        public GeneralDiscreteDistribution(IList<double> probas)
            : base()
        {
            if (probabilities_ == null)
            {
                probabilities_ = new List<double>();
            }
            probabilities_.Clear();
            for (int i = 0; i < probas.Count; i++)
            {
                probabilities_.Add(probas[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<double>? Probabilities
        {
            get { return probabilities_; }
            set
            {
                probabilities_ = value;
            }
        }
        public bool Equals(GeneralDiscreteDistribution? cmp)
        {
            bool eq = base.Equals(cmp);
            if (cmp != null)
            {
                if (Probabilities != null && cmp.Probabilities != null)
                {
                    eq &= Probabilities.Count == cmp.Probabilities.Count;
                    if (eq)
                    {
                        for (int i = 0;i < Probabilities.Count;i++)
                        {
                            eq &= Numeric.EQ(Probabilities[i], cmp.Probabilities[i]);  
                        }
                    }
                }
                else
                {
                    eq &= Probabilities == null && cmp.Probabilities == null;
                }
            }
            return eq;
        }
        public void CopyTo(GeneralDiscreteDistribution? dest)
        {
            base.CopyTo(dest);
            if (dest != null)
            {
                if (Probabilities != null)
                {
                    if (dest.Probabilities == null)
                    {
                        dest.Probabilities = new List<double>();
                    }
                    dest.Probabilities.Clear();
                    for (int i = 0; i  < Probabilities.Count;i++)
                    {
                        dest.Probabilities.Add(Probabilities[i]);
                    }
                }
                else
                {
                    dest.Probabilities = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            double total = 0;
            if (probabilities_ != null)
            {
                for (int i = 0; i < probabilities_.Count; i++)
                {
                    if (probabilities_[i] < 0 || probabilities_[i] > 1)
                    { return false; }
                    total += probabilities_[i];
                }
            }
            return (total == 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            double mean = 0;
            if (probabilities_ != null)
            {
                for (int i = 0; i < probabilities_.Count; i++)
                {
                    if (probabilities_[i] < 0 || probabilities_[i] > 1)
                    {
                        return null;
                    }
                    mean += i * probabilities_[i];
                }
            }
            return mean;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int? Realize()
        {
            double random = RandomGenerator.Instance.NextDouble();
            double total = 0;
            if (probabilities_ != null)
            {
                for (int i = 0; i < probabilities_.Count; i++)
                {
                    if (random > total && random < total + probabilities_[i])
                    {
                        return i;
                    }
                    total += probabilities_[i];
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(DiscreteDistribution from)
        {
            if (from is GeneralDiscreteDistribution gd)
            {
                if (probabilities_ != null)
                {
                    probabilities_.Clear();
                }
                if (gd.probabilities_ != null)
                {
                    if (probabilities_ == null)
                    {
                        probabilities_ = new List<double>();
                    }
                    for (int i = 0; i < gd.probabilities_.Count; i++)
                    {
                        probabilities_.Add(gd.probabilities_[i]);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteDistribution Clone()
        {
            GeneralDiscreteDistribution gd = new GeneralDiscreteDistribution();
            gd.probabilities_ = new List<double>();
            if (probabilities_ != null)
            {
                for (int i = 0; i < probabilities_.Count; i++)
                {
                    gd.probabilities_.Add(probabilities_[i]);
                }
            }
            return gd;
        }
    }
}
