using System;
using System.Collections;
using System.Collections.Generic;

namespace STLib.Json
{
    public delegate void STJsonCreatorCallback();
    public delegate void STJsonCreatorForCallback(int n_index);
    public delegate void STJsonCreatorForEachCallback<T>(T v);
    public delegate void STJsonCreatorStartCallback(STJsonCreator writer);

    public class STJsonCreatorException : STJsonException
    {
        public STJsonCreatorException(string str_error) : base(str_error) { }
    }

    public class STJsonCreator
    {
        private bool m_b_started;
        private STJson m_current_json;

        private Stack<STJson> m_stack;

        public STJsonCreator()
        {
            m_stack = new Stack<STJson>();
        }

        public STJson Create(STJsonCreatorStartCallback callback)
        {
            if (m_b_started) {
                m_b_started = false;
                throw new STJsonCreatorException("Please wait Create() end.");
            }
            m_stack.Clear();
            m_b_started = true;
            this.Push(new STJson());
            callback(this);
            m_b_started = false;
            return m_current_json;
        }

        public STJsonCreator SetItem(string str_key, long value)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, value);
            return this;
        }

        public STJsonCreator SetItem(string str_key, double value)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, value);
            return this;
        }

        public STJsonCreator SetItem(string str_key, bool value)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, value);
            return this;
        }

        public STJsonCreator SetItem(string str_key, string value)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, value);
            return this;
        }

        public STJsonCreator SetItem(string str_key, DateTime value)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, value);
            return this;
        }

        public STJsonCreator SetItem(string str_key, STJson json)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, json);
            return this;
        }

        public STJsonCreator SetItem(string str_key, object obj)
        {
            this.CheckStarted();
            m_current_json.SetItem(str_key, obj);
            return this;
        }

        public STJsonCreator SetItem(string str_key, bool b_express, STJsonCreatorCallback callback)
        {
            this.CheckStarted();
            if (!b_express) return this;
            var json = new STJson();
            m_current_json.SetItem(str_key, json);

            this.Push(json);
            callback();
            this.Pop();
            return this;
        }

        public STJsonCreator Append(long value)
        {
            this.CheckStarted();
            m_current_json.Append(value);
            return this;
        }

        public STJsonCreator Append(double value)
        {
            this.CheckStarted();
            m_current_json.Append(value);
            return this;
        }

        public STJsonCreator Append(bool value)
        {
            this.CheckStarted();
            m_current_json.Append(value);
            return this;
        }

        public STJsonCreator Append(string value)
        {
            this.CheckStarted();
            m_current_json.Append(value);
            return this;
        }

        public STJsonCreator Append(DateTime value)
        {
            this.CheckStarted();
            m_current_json.Append(value);
            return this;
        }

        public STJsonCreator Append(object obj)
        {
            this.CheckStarted();
            m_current_json.Append(obj);
            return this;
        }

        public STJsonCreator Append(STJson json)
        {
            this.CheckStarted();
            m_current_json.Append(json);
            return this;
        }

        public STJsonCreator Append(STJsonCreatorCallback callback)
        {
            this.CheckStarted();
            var json = new STJson();
            m_current_json.Append(json);

            this.Push(json);
            callback();
            this.Pop();
            return this;
        }

        public STJsonCreator Append(int n_count, STJsonCreatorForCallback callback)
        {
            return this.Append(0, n_count, 1, callback);
        }

        public STJsonCreator Append(int n_start, int n_end, STJsonCreatorForCallback callback)
        {
            return this.Append(n_start, n_end, 1, callback);
        }

        public STJsonCreator Append(int n_start, int n_end, int n_step, STJsonCreatorForCallback callback)
        {
            this.CheckStarted();
            for (int i = n_start; i < n_end; i += n_step) {
                var json = new STJson();
                m_current_json.Append(json);

                this.Push(json);
                callback(i);
                this.Pop();
            }
            return this;
        }

        public STJsonCreator Append<T>(Array arr, STJsonCreatorForEachCallback<T> callback)
        {

            return this.Append<T>(arr.GetEnumerator(), callback);
        }

        public STJsonCreator Append<T>(IList lst, STJsonCreatorForEachCallback<T> callback)
        {

            return this.Append<T>(lst.GetEnumerator(), callback);
        }

        public STJsonCreator Append<T>(IList<T> lst, STJsonCreatorForEachCallback<T> callback)
        {

            return this.Append<T>(lst.GetEnumerator(), callback);
        }

        public STJsonCreator Append<T>(IEnumerator enumerator, STJsonCreatorForEachCallback<T> callback)
        {
            return this.Append<T>(enumerator, callback);
        }

        public STJsonCreator Append<T>(IEnumerator<T> enumerator, STJsonCreatorForEachCallback<T> callback)
        {
            this.CheckStarted();
            while (enumerator.MoveNext()) {
                var json = new STJson();
                m_current_json.Append(json);

                this.Push(json);
                callback(enumerator.Current);
                this.Pop();
            }
            return this;
        }

        public STJsonCreator Append<T>(IEnumerable enumerable, STJsonCreatorForEachCallback<T> callback)
        {
            return this.Append<T>(enumerable, callback);
        }

        public STJsonCreator Append<T>(IEnumerable<T> enumerable, STJsonCreatorForEachCallback<T> callback)
        {
            this.CheckStarted();
            foreach (var v in enumerable) {
                var json = new STJson();
                m_current_json.Append(json);

                this.Push(json);
                callback(v);
                this.Pop();
            }
            return this;
        }

        public STJsonCreator Set(string str_path, object obj)
        {
            this.CheckStarted();
            m_current_json.Set(str_path, obj);
            return this;
        }

        public STJsonCreator Set(string str_path, STJson json)
        {
            this.CheckStarted();
            m_current_json.Set(str_path, json);
            return this;
        }

        public STJsonCreator Set(string str_path, STJsonCreatorCallback callback)
        {
            return this.Set(str_path, true, callback);
        }

        public STJsonCreator Set(string str_path, bool b_express, STJsonCreatorCallback callback)
        {
            this.CheckStarted();
            if (!b_express) return this;
            var json = new STJson();
            m_current_json.Set(str_path, json);

            this.Push(json);
            callback();
            this.Pop();
            return this;
        }


        private void Push(STJson json)
        {
            m_current_json = json;
            m_stack.Push(json);
        }

        private void Pop()
        {
            m_current_json = null;
            if (m_stack.Count == 0) return;
            m_stack.Pop();
            if (m_stack.Count == 0) return;
            m_current_json = m_stack.Peek();
        }

        private void CheckStarted()
        {
            if (m_b_started) {
                return;
            }
            throw new STJsonCreatorException("Please call Create() first.");
        }
    }
}
