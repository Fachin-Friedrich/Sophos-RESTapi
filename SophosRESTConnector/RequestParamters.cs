using System;

namespace SophosRESTConnector
{
    public enum TimeConstraintType
    {
        Before,
        After
    }

    public class TimeConstraint
    {
        public TimeConstraintType constrainttype;
        public DateTime when;

    } //End of class

} //End of namespace
