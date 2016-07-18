﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZV.LinqExtentions
{
    public class WhereClause : IWhereClause
    {
        public string Field { get; set; }
        public WhereOperator Operator { get; set; }
        public object Value { get; set; }

        public WhereClause() {
        }

        public WhereClause(string field, WhereOperator op, object value)
        {
            this.Field = field;
            this.Operator = op;
            this.Value = value;
        }
    }
}
