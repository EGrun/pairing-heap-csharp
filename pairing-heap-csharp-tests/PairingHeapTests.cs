using System;
using System.Collections.Generic;
using System.Linq;
using pairing_heap_csharp;
using Xunit;
using Shouldly;

namespace pairing_heap_csharp_tests
{
    public class PairingHeapTests
    {
        [Fact]
        public void Constructor_NoParameters_ShouldNotThrow()
        {
            // Arrange, Act
            var heap = new PairingHeap<Object>();

            // Assert
            this.ShouldSatisfyAllConditions(
                () => heap.ShouldNotBeNull(),
                () => heap.ShouldBeOfType(typeof(PairingHeap<Object>)));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldNotThrow()
        {
            // Arrange, Act
            var heap = new PairingHeap<string>((a, b) => a.CompareTo(b));

            // Assert
            this.ShouldSatisfyAllConditions(
                () => heap.ShouldNotBeNull(),
                () => heap.ShouldBeOfType(typeof(PairingHeap<string>)));
        }

        [Fact]
        public void ToList_EmptyHeap_ShouldReturnEmptyList()
        {
            // Arrange
            var heap = new PairingHeap<string>();

            // Act
            var list = heap.ToList();

            // Assert
            list.Count.ShouldBe(0);
        }

        [Fact]
        public void Insert_SingleItem_ShouldContainSingleItem()
        {
            // Arrange
            var heap = new PairingHeap<string>();

            // Act
            heap.Insert("A");

            // Assert
            heap.ToList().Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(10000)]
        [InlineData(10000000)]
        public void Insert_MultipleItems_ShouldContainCorrectItems(int numToInsert)
        {
            // Arrange
            var heap = new PairingHeap<string>();

            // Act
            for (var i = 1; i <= numToInsert; ++i)
            {
                heap.Insert(i.ToString());
            }

            // Assert
            heap.ToList().Count.ShouldBe(numToInsert);
        }

        [Fact]
        public void ExtractMin_ShouldReturnItemsInCorrectOrder()
        {
            // Arrange
            var heap = new PairingHeap<string>();
            var stringToInsert = "ALPHABETICAL";

            // Act
            for (var i = 0; i < stringToInsert.Length; ++i)
            {
                heap.Insert(stringToInsert.Substring(i,1));
            }

            var list = new List<string>();

            while (heap.Count > 0)
            {
                list.Add(heap.ExtractMin());
            }

            var output = list.Aggregate("", (a, b) => a + b);
            
            Console.WriteLine(output);

            // Assert
            output.ShouldBe("AAABCEHILLPT");
        }

        public enum TestAction
        {
            Insert,
            ExtractMin,
            UpdateKeyValue
        }

        public static TheoryData<(TestAction, int, int)[]> InOrderInsertTestData =>
            new TheoryData<(TestAction, int, int)[]>
            {
                new [] 
                {
                    (TestAction.Insert, 1, 1),
                    (TestAction.Insert, 2, 2)
                }
            };

        public static TheoryData<(TestAction, int, int)[]> ReverseOrderInsertTestData =>
            new TheoryData<(TestAction, int, int)[]>
            {
                new []
                {
                    (TestAction.Insert, 1, 2),
                    (TestAction.Insert, 2, 1)
                }
            };

        public static TheoryData<(TestAction, int, int)[]> InsertExtractInsertTestData =>
            new TheoryData<(TestAction, int, int)[]>
            {
                new []
                {
                    (TestAction.Insert, 1, 1),
                    (TestAction.Insert, 2, 2),
                    (TestAction.ExtractMin, 0, 0),
                    (TestAction.Insert, 3, 3),
                }
            };

        public static TheoryData<(TestAction, int, int)[]> UpdateItemTestData =>
            new TheoryData<(TestAction, int, int)[]>
            {
                new []
                {
                    (TestAction.Insert, 1, 1),
                    (TestAction.Insert, 2, 2),
                    (TestAction.Insert, 3, 3),
                    (TestAction.UpdateKeyValue, 2, 5)
                },
                new []
                {
                    (TestAction.Insert, 1, 5),
                    (TestAction.Insert, 2, 10),
                    (TestAction.Insert, 3, 15),
                    (TestAction.UpdateKeyValue, 2, 1)
                },
                new []
                {
                    (TestAction.Insert, 1, 5),
                    (TestAction.Insert, 2, 10),
                    (TestAction.Insert, 3, 20),
                    (TestAction.Insert, 4, 30),
                    (TestAction.ExtractMin, 0, 0),
                    (TestAction.UpdateKeyValue, 3, 1)
                },
            };

        [Theory]
        [MemberData(nameof(InOrderInsertTestData))]
        [MemberData(nameof(ReverseOrderInsertTestData))]
        [MemberData(nameof(InsertExtractInsertTestData))]
        [MemberData(nameof(UpdateItemTestData))]
        public void PriorityQueue_ShouldMaintainMinOrder((TestAction action, int id, int priority)[] testActions)
        {
            // Arrange
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));

            // Act
            var output = new List<TestTask>();
            foreach (var testAction in testActions)
            {
                switch (testAction.action)
                {
                    case TestAction.Insert:
                        heap.Insert(new TestTask() {Id = testAction.id, Priority = testAction.priority});
                        break;
                    case TestAction.ExtractMin:
                        output.Add(heap.ExtractMin());
                        break;
                    case TestAction.UpdateKeyValue:
                        var task = new TestTask() {Id = testAction.id, Priority = testAction.priority};
                        var node = heap.Find(task);
                        heap.UpdateItem(node, task);
                        break;
                    default:
                        break;
                }
            }

            var list = new List<TestTask>();
            while (heap.Count > 0)
            {
                list.Add(heap.ExtractMin());
            }

            // Assert
            list.OrderBy(n => n.Priority).SequenceEqual(list).ShouldBeTrue();
        }

        [Fact]
        public void Find_OnHead_ShouldReturnHead()
        {
            // Arrange
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));
            var task = new TestTask()
            {
                Id = 1,
                Priority = 10
            };
            var node = heap.Insert(task);

            // Act
            var foundNode = heap.Find(task);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => foundNode.ShouldBe(node)
            );
        }

        [Fact]
        public void Find_OnMemberNode_ShouldReturnNode()
        {
            // Arrange
            var heap = new PairingHeap<TestTask>((a,b) => a.Priority.CompareTo(b.Priority));
            var task = new TestTask()
            {
                Id = 1,
                Priority = 10
            };
            var node = heap.Insert(task);

            var numToInsert = 99;
            for (var i = 0; i < numToInsert; ++i)
            {
                heap.Insert(new TestTask() { Id = i+2, Priority = i});
            }

            // Act
            var foundNode = heap.Find(task);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => foundNode.ShouldBe(node)
            );
        }

        [Fact]
        public void Find_OnNonMemberNode_ShouldReturnNull()
        {
            // Arrange
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));
            var task = new TestTask()
            {
                Id = 1,
                Priority = 10
            };

            var numToInsert = 99;
            var rand = new Random();
            for (var i = 0; i < numToInsert; ++i)
            {
                heap.Insert(new TestTask() { Id = i+2, Priority = rand.Next(0,100) });
            }

            // Act
            var foundNode = heap.Find(task);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => foundNode.ShouldBe(null)
            );
        }

        [Fact]
        public void Find_OnEmptyHeap_ShouldThrow()
        {
            // Arrange, Act
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));
            var task = new TestTask()
            {
                Id = 1,
                Priority = 10
            };

            // Assert
            Should.Throw<InvalidOperationException>(() => heap.Find(task));
        }

        [Fact]
        public void UpdateItem_OnHead_ShouldSucceed()
        {
            // Arrange
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));
            var task = new TestTask()
            {
                Id = 1,
                Priority = 10
            };

            var node = heap.Insert(task);

            // Act
            task.Priority--;
            heap.UpdateItem(node, task);

            // Assert
            var extracted = heap.ExtractMin();
            this.ShouldSatisfyAllConditions(
                () => extracted.ShouldBe(task),
                () => extracted.Priority.ShouldBe(9)
            );
        }

        [Fact]
        public void UpdateItem_BelowTopPriority_ShouldFlipNodesInQueue()
        {
            // Arrange
            var task = new TestTask()
            {
                Id = 1,
                Priority = 10
            };
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));
            heap.Insert(new TestTask() {Priority = 5});

            var node = heap.Insert(task);

            // Act
            task.Priority = 1;
            heap.UpdateItem(node, task);

            // Assert
            var extracted = heap.ExtractMin();
            this.ShouldSatisfyAllConditions(
                () => extracted.ShouldBe(task),
                () => extracted.Priority.ShouldBe(1)
            );
        }

        [Fact]
        public void ExamineMin_OnEmptyHeap_ShouldThrow()
        {
            // Arrange, Act
            var heap = new PairingHeap<TestTask>();

            // Assert
            Should.Throw<InvalidOperationException>(() => heap.ExamineMin());
        }

        [Fact]
        public void ExamineMin_OnPopulatedHeap_ShouldReturnMinimalItem()
        {
            // Arrange
            var heap = new PairingHeap<TestTask>((a, b) => a.Priority.CompareTo(b.Priority));

            var task1 = new TestTask()
            {
                Id = 1,
                Priority = 5
            };

            var task2 = new TestTask()
            {
                Id = 2,
                Priority = 1
            };

            heap.Insert(task1);
            heap.Insert(task2);

            // Act
            var min = heap.ExamineMin();

            // Assert
            min.ShouldBe(task2);
        }

        class TestTask
        {
            public int Priority { get; set; }
            public int Id { get; set; }

            public override bool Equals(object obj)
            {
                var task = obj as TestTask;
                if (object.ReferenceEquals(task, null))
                {
                    return false;
                }

                return this.Equals(task);
            }

            private bool Equals (TestTask task)
            {
                return this.Id == task.Id;
            }

            public static bool operator ==(TestTask task1, TestTask task2)
            {
                if (object.ReferenceEquals(task1, null))
                {
                    return object.ReferenceEquals(task2, null);
                }

                return task1.Equals(task2);
            }

            public static bool operator !=(TestTask task1, TestTask task2)
            {
                if (object.ReferenceEquals(task1, null))
                {
                    return !object.ReferenceEquals(task2, null);
                }

                return !task1.Equals(task2);
            }
        }
    }
}
