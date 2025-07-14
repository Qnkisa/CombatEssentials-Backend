using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Domain.Validations
{
    public static class ReviewValidations
    {
        public const int RatingMinValue = 1;
        public const int RatingMaxValue = 5;
        public const int CommentMaxLength = 1000; 
    }
}
