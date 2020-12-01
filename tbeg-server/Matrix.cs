using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;

namespace TBeg
{
    public  class Matrix<TOne,TTwo>
    {

        /// <summary>
        /// The matrix-entries.
        /// </summary>
        public TTwo[,] values;// TOne is not the current value of the matrix, T depends on branching type

        public string[] alphabet;

        public IFunctor<TOne,TTwo> functor;

        private Graph InitGraph;

        public string[] Alphabet
        {
            get
            {
                return alphabet;
            }

            set
            {
                alphabet = value;
            }
        }

        public Graph InitGraph1
        {
            get
            {
                return InitGraph;
            }

            set
            {
                InitGraph = value;
            }
        }

        public Matrix(int rows, int columns, IFunctor<TOne,TTwo> functor)
        {
            this.functor = functor;
            values = new TTwo[rows, columns];
            for (int i = 0; i < rows; ++i) for (int j = 0; j < columns; ++j) values[i, j] =  Zero();
        }

        //TODO set values:
        //should depend on method in functor?
        /// <summary>
        /// Set an individual entry of the matrix to a provided value.
        /// </summary>
        /// <param name="row">The row index of the entry to be set.</param>
        /// <param name="column">The column index of the entry to be set.</param>
        /// <param name="value">The value the entry gets.</param>
        public void set(int row, int column, TTwo value)
        {
            values[row, column] = value;
        }

        public TTwo get(int row, int column)
        {
            return values[row, column];
        }

        



        /// <summary>
        /// Yields the one of the semiring. Either T needs to be of type Semiring or int, uint, double for this to work.
        /// </summary>
        /// <returns>The one of the given semiring.</returns>
        public static TTwo One()
        {
            if ((typeof(TTwo) == typeof(int)) || (typeof(TTwo) == typeof(uint)) || (typeof(TTwo) == typeof(double))) return (dynamic)1;
            try
            {
                Type t = typeof(TTwo);

                return (dynamic)t.GetMethod("One").Invoke(null, null);
            }
            catch (Exception)
            {
                throw (new Exception("One for type " + typeof(TTwo).ToString() + " not defined! Define function One() in " + typeof(TTwo).ToString() + " that yields the one of said semiring."));
            };


        }
        /// <summary>
        /// Yields the zero of the semiring. Either T needs to be of type Semiring or int, uint, double for this to work.
        /// </summary>
        /// <returns>The zero of the given semiring.</returns>

        public static TTwo Zero()
        {
            if ((typeof(TTwo) == typeof(int)) || (typeof(TTwo) == typeof(uint)) || (typeof(TTwo) == typeof(double))) return (dynamic)0;
            try
            {
                Type t = typeof(TTwo);

                return (dynamic)t.GetMethod("Zero").Invoke(null, null);
            }
            catch (Exception)
            {
                throw (new Exception("Zero for type " + typeof(TTwo).ToString() + " not defined! Define function Zero() in " + typeof(TTwo).ToString() + " that yields the zero of said semiring."));
            };
        }
        /// <summary>
        /// Checks if a value equals to Zero of this structure
        /// </summary>
        /// <returns> returns true if Zero </returns>
        public  bool EqualToZero(int r, int c)
        {
            TTwo val = get(r, c);
            if ((dynamic)val == Zero()) return true;
            return false;
        }


        /// <summary>
        /// Checks if a value equals to One of this structure
        /// </summary>
        /// <returns> true if One </returns>
        public bool EqualToOne(int r, int c)
        {
            TTwo val = get(r, c);
            if ((dynamic)val == One()) return true;
            return false;
        }

        /// <summary>
        /// Compares two matrices element wise and yields true if and only if each entry is equal.
        /// </summary>
        /// <param name="m1">The first matrix to be compared.</param>
        /// <param name="m2">The second matrix to be compared.</param>
        /// <returns>True if the matrices are equal, false otherwise.</returns>
        public static bool operator ==(Matrix<TOne, TTwo> m1, Matrix<TOne, TTwo> m2)
        {
            if (m1.rows() != m2.rows() || m1.columns() != m2.columns()) return false;
            for (int i = 0; i < m1.rows(); ++i)
            {
                for (int j = 0; j < m2.columns(); ++j)
                {
                    if ((dynamic)m1.get(i, j) != (dynamic)m2.get(i, j)) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares two matrices element wise and yields true if and only if each entry is not equal.
        /// </summary>
        /// <param name="m1">The first matrix to be compared.</param>
        /// <param name="m2">The second matrix to be compared.</param>
        /// <returns>False if the matrices are equal, true otherwise.</returns>
        public static bool operator !=(Matrix<TOne, TTwo> m1, Matrix<TOne, TTwo> m2)
        {
            return !(m1 == m2);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            return this == (Matrix<TOne, TTwo>)obj;
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return base.GetHashCode();
        }

        /// <summary>
        /// Gives the number of rows the matrix has.
        /// </summary>
        /// <returns>The number of rows the matrix has.</returns>
        public int rows()
        {
            return values.GetUpperBound(0) + 1;
        }

        /// <summary>
        /// Gives the number of columns the matrix has.
        /// </summary>
        /// <returns>The number of columns the matrix has.</returns>
        public int columns()
        {
            return values.GetUpperBound(1) + 1;
        }

        //TODO: only for debug purposes
        public void print() {

            int rowLength = values.GetLength(0);
            int colLength = values.GetLength(1);

            for (int i = 0; i < rowLength; i++) {
                for (int j = 0; j < colLength; j++) {
                    Console.Write(string.Format("{0} ", values[i, j]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }


    }
}