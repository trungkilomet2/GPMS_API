using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Common
{
    public static class ValidationField
    {   
        public static void AddFieldError(IDictionary<string, string[]> errors, string field, string message)
        {
            if (errors.ContainsKey(field))
            {
                var existingErrors = errors[field].ToList();
                existingErrors.Add(message);
                errors[field] = existingErrors.ToArray();
            }
            else
            {
                errors[field] = new string[] { message };
            }
        }
    }
}
