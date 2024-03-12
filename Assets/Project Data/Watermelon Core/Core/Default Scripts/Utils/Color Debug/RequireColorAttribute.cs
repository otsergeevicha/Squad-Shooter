using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireColorAttribute : Attribute
    {
        public string name;        

        public RequireColorAttribute(string name)
        {
            this.name = name;           
        }
    }
}
