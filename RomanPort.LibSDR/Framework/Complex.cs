using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public struct Complex
    {
        public float Real;
        public float Imag;

        public static readonly Complex Zero = new Complex(0.0f, 0.0f);
        public static readonly Complex One = new Complex(1.0f, 0.0f);
        public static readonly Complex Two = new Complex(2.0f, 0.0f);
        public static readonly Complex ImaginaryOne = new Complex(0.0f, 1.0f);

        public Complex(float real, float imaginary)
        {
            Real = real;
            Imag = imaginary;
        }

        public Complex(Complex c)
        {
            Real = c.Real;
            Imag = c.Imag;
        }

        public float Modulus()
        {
            return MathF.Sqrt(ModulusSquared());
        }

        public float ModulusSquared()
        {
            return Real * Real + Imag * Imag;
        }

        public float Argument()
        {
            return MathF.Atan2(Imag, Real);
        }

        public float ArgumentFast()
        {
            return Trig.Atan2(Imag, Real);
            //return (ImaginaryOne / Two) * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
        }

        public Complex Conjugate()
        {
            return new Complex(Real, -Imag);
        }

        public Complex Normalize()
        {
            var norm = 1.0f / Modulus();
            return this * norm;
        }

        public Complex NormalizeFast()
        {
            var norm = 1.95f - ModulusSquared();
            return this * norm;
        }
        /*public Complex Log()
        {
            //return (new Complex((Math.Log(FloatingMathF.Abs(this))), (MathF.Atan2(Imag, Real))));
        }*/

        public override string ToString()
        {
            return string.Format("real {0}, imag {1}", Real, Imag);
        }

        public static Complex FromAngle(float angle)
        {
            Complex result;
            result.Real = MathF.Cos(angle);
            result.Imag = MathF.Sin(angle);
            return result;
        }

        public static Complex FromAngleFast(float angle)
        {
            return Trig.SinCos(angle);
        }

        public static bool operator ==(Complex leftHandSide, Complex rightHandSide)
        {
            if (leftHandSide.Real != rightHandSide.Real)
            {
                return false;
            }
            return (leftHandSide.Imag == rightHandSide.Imag);
        }

        public static bool operator !=(Complex leftHandSide, Complex rightHandSide)
        {
            if (leftHandSide.Real != rightHandSide.Real)
            {
                return true;
            }
            return (leftHandSide.Imag != rightHandSide.Imag);
        }

        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imag + b.Imag);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imag - b.Imag);
        }

        public static Complex operator -(Complex value)
        {
            return (new Complex((-value.Real), (-value.Imag)));
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Real * b.Real - a.Imag * b.Imag,
                               a.Imag * b.Real + a.Real * b.Imag);
        }

        public static Complex operator *(Complex a, float b)
        {
            return new Complex(a.Real * b, a.Imag * b);
        }

        public static Complex operator /(Complex a, Complex b)
        {
            var dn = b.Real * b.Real + b.Imag * b.Imag;
            dn = 1.0f / dn;
            var re = (a.Real * b.Real + a.Imag * b.Imag) * dn;
            var im = (a.Imag * b.Real - a.Real * b.Imag) * dn;
            return new Complex(re, im);
        }

        public static Complex operator /(Complex a, float b)
        {
            b = 1f / b;
            return new Complex(a.Real * b, a.Imag * b);
        }

        public static Complex operator ~(Complex a)
        {
            return a.Conjugate();
        }

        public static implicit operator Complex(float d)
        {
            return new Complex(d, 0);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Real.GetHashCode() * 397) ^ Imag.GetHashCode();
            }
        }

        public bool Equals(Complex obj)
        {
            return obj.Real == Real && obj.Imag == Imag;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Complex)) return false;
            return Equals((Complex)obj);
        }
    }
}
