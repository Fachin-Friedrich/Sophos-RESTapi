using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SophosRESTConnector.Requests
{
    public interface IRequestParameter
    {
        string GetRequestString();
    }
    
    internal class ConstantRequestParameter : IRequestParameter
    {
        private string val;
        public ConstantRequestParameter(string str) => val = str;
        public string GetRequestString() => val;

        public static ConstantRequestParameter EmptyRequestParameter = new ConstantRequestParameter(string.Empty);
    }

    public enum TimeConstraintType
    {
        Before,
        After
    }

    public class TimeParameter : IRequestParameter
    {
        public TimeConstraintType constrainttype;
        public DateTime when;

        public TimeParameter(DateTime time, TimeConstraintType constraint)
        => (when, constrainttype) = (time, constraint);

        public TimeParameter(int day, int month, int year, TimeConstraintType constraint)
        => (when, constrainttype) = (new DateTime(year, month, day),constraint);


        public TimeParameter(TimeConstraintType constraint) 
        => (when, constrainttype) = (DateTime.Now, constraint); 

        public string GetRequestString()
        {
            string fmt = $"{when.Year}-{when.Month}-{when.Day}T{when.Hour}:{when.Minute}:{when.Second}.{when.Millisecond}Z";
            
            switch(constrainttype)
            {
                case TimeConstraintType.Before:
                    return $"to={fmt}";

                case TimeConstraintType.After:
                    return $"?from={fmt}";

                default:
                    throw new ArgumentException($"Invalid Timeconstraint ({constrainttype}) provided");
            }
        }

    } //End of class

    public class CompoundParameter : HashSet<IRequestParameter>, IRequestParameter
    {
        public string GetRequestString()
        {
            StringBuilder sb = new StringBuilder();
            foreach( var str in this)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }
    }

} //End of namespace
