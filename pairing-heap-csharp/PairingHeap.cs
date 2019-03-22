using System;
using System.Collections.Generic;

namespace pairing_heap_csharp
{
    public sealed class PairingNode<T>
    {
        public T Item { get; }
        public PairingNode<T> Parent { get; internal set; }
        public PairingNode<T> Sibling { get; internal set; }
        public PairingNode<T> Child { get; internal set; }

        public PairingNode(T item)
        {
            if (object.ReferenceEquals(item, null))
            {
                throw new ArgumentNullException(nameof(item));
            }

            Item = item;
            Parent = null;
            Sibling = null;
            Child = null;
        }

        public void Iterate(Func<PairingNode<T>, bool> action)
        {
            if (object.ReferenceEquals(action, null))
            {
                throw new ArgumentNullException(nameof(action));
            }

            var stack = new Stack<PairingNode<T>>();
            stack.Push(this);

            while (stack.Count != 0)
            {
                var node = stack.Pop();
                var shouldContinue = action(node);

                if (!shouldContinue)
                {
                    break;
                }

                if (!object.ReferenceEquals(node.Sibling, null))
                {
                    stack.Push(node.Sibling);
                }

                if (!object.ReferenceEquals(node.Child, null))
                {
                    stack.Push(node.Child);
                }
            }
        }

        internal void AddChild(PairingNode<T> node)
        {
            node.Parent = this;
            node.Sibling = this.Child;
            this.Child = node;
        }
    }

    public sealed class PairingHeap<T>
    {
        private PairingNode<T> _head;
        private readonly Func<T, T, int> _comparer;
        private int _count;
       
        public PairingHeap() 
        : this(Comparer<T>.Default.Compare)
        {
        }
        
        public PairingHeap (Func<T, T, int> compare)
        {
            _comparer = compare ?? throw new ArgumentNullException(nameof(compare));
            _head = null;
            _count = 0;
        }

        private PairingNode<T> Link(PairingNode<T> node1, PairingNode<T> node2)
        {
            if (object.ReferenceEquals(node1, null))
            {
                return node2;
            }

            if (object.ReferenceEquals(node2, null))
            {
                return node1;
            }

            if (_comparer(node1.Item, node2.Item) > 0)
            {
                node2.AddChild(node1);
                return node2;
            }
            else
            {
                node1.AddChild(node2);
                return node1;
            }
        }

        private PairingNode<T> Extract(PairingNode<T> node)
        {
            var children = new List<PairingNode<T>>();

            var n = node.Child;
            while (!object.ReferenceEquals(n, null))
            {
                var pair = n.Sibling;
                n.Parent = null;
                n.Sibling = null;
                if (object.ReferenceEquals(pair, null))
                {
                    children.Add(n);
                    break;
                }

                var next = pair.Sibling;
                pair.Parent = null;
                pair.Sibling = null;
                children.Add(Link(n, pair));
                n = next;
            }

            if (children.Count == 0)
            {
                return null;
            }

            var root = children[0];
            for (var i = 1; i < children.Count; ++i)
            {
                root = Link(root, children[i]);
            }

            return root;
        }

        public int Count
        {
            get { return _count; }
        }

        public PairingNode<T> Insert(T item)
        {
            if (object.ReferenceEquals(item, null))
            {
                throw new ArgumentNullException(nameof(item));
            }

            var node = new PairingNode<T>(item);
            if (object.ReferenceEquals(_head, null))
            {
                _head = node;
            }
            else
            {
                _head = Link(_head, node);
            }

            _count++;
            return node;
        }

        public T ExamineMin()
        {
            if (object.ReferenceEquals(_head, null))
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            return _head.Item;
        }

        public T ExtractMin()
        {
            if (object.ReferenceEquals(_head, null))
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            var node = _head;
            _head = Extract(_head);
            _count--;
            return node.Item;
        }

        public PairingNode<T> Find(T item)
        {
            if (object.ReferenceEquals(_head, null))
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            PairingNode<T> foundItem = null;
            _head.Iterate((n) =>
            {
                if (EqualityComparer<T>.Default.Equals(n.Item, item))
                {
                    foundItem = n;
                    return false;
                }
                else
                {
                    return true;
                }
            });
            return foundItem;
        }

        public PairingNode<T> UpdateItem(PairingNode<T> node, T item)
        {
            if (object.ReferenceEquals(node, null))
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (object.ReferenceEquals(item, null))
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_head == node)
            {
                ExtractMin();
                return Insert(item);
            }
            else
            {
                var child = Extract(node);
                if (!object.ReferenceEquals(child, null))
                {
                    child.Sibling = node.Parent.Child;
                    node.Parent.Child = child;
                }
                return Insert(item);
            }
        }

        public List<PairingNode<T>> ToList()
        {
            var list = new List<PairingNode<T>>();
            if (object.ReferenceEquals(_head, null))
            {
                return list;
            }

            _head.Iterate((node) =>
            {
                list.Add(node);
                return true;
            });

            return list;
        }
    }
}
