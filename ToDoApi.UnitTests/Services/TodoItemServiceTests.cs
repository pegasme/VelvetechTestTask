﻿using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TodoApi.Data.Interfaces;
using TodoApi.Data.Models;
using TodoApi.Services.Exceptions;
using TodoApi.Services.Models;
using TodoApi.Services.Services;
using TodoApi.Services.Services.Interfaces;

namespace TodoApi.UnitTests.Services
{
    [TestClass]
    public class TodoItemServiceTests
    {
        private TodoItemService _service;
        private IRepository<TodoItem, long> _repository;
        private ITodoItemMappingService _todoItemMappingService;

        [TestInitialize]
        public void SetUp()
        {
            _repository = Substitute.For<IRepository<TodoItem, long>>();
            _todoItemMappingService = Substitute.For<ITodoItemMappingService>();
            _service = new TodoItemService(_repository, _todoItemMappingService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _repository.ClearReceivedCalls();
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnTodoItems_IfExists()
        {
            //Arrange
            var items = new Fixture().Create<ReadOnlyCollection<TodoItem>>();
            var itemsResult = new Fixture().Create<ReadOnlyCollection<TodoItemDTO>>();
            _repository.GetAsync().Returns(items);
            _todoItemMappingService.MapTodoItemToDTO(items).Returns(itemsResult);

            // Act
            var result = await _service.GetAsync();

            //Assert
            Assert.AreEqual(result, itemsResult);
            await _repository.Received(1).GetAsync();
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnTodoItem_IfExists()
        {
            //Arrange
            var item = new Fixture().Create<TodoItem>();
            var itemResult = new Fixture().Create<TodoItemDTO>();
            _repository.GetAsync(item.Id).Returns(item);
            _todoItemMappingService.MapTodoItemToDTO(item).Returns(itemResult);

            // Act
            var result = await _service.GetAsync(item.Id);

            //Assert
            Assert.AreEqual(result, itemResult);
            await _repository.Received(1).GetAsync(item.Id);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnNotFoundException_IfNotExists()
        {
            //Arrange
            var item = new Fixture().Create<TodoItem>();

            // Act
            await Assert.ThrowsExceptionAsync<NotFoundException>(async () => await _service.GetAsync(item.Id));

            //Assert
            await _repository.Received(1).GetAsync(item.Id);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public async Task GetAsync_ShouldReturnArgumentException_IfIdIncorrect(long id)
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _service.GetAsync(id));

            //Assert
            await _repository.DidNotReceive().GetAsync(id);
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCreateItem_IfCorrect()
        {
            //Arrange
            var newItem = new Fixture().Create<TodoItemDTO>();
            _repository.SaveAsync().Returns(Task.CompletedTask);
            _todoItemMappingService.MapTodoItemToDTO(Arg.Any<TodoItem>()).Returns(newItem);

            // Act
            var result = await _service.CreateAsync(newItem);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Name, newItem.Name);
            Assert.AreEqual(result.IsComplete, newItem.IsComplete);

            _repository.Received(1).Create(Arg.Any<TodoItem>());
            await _repository.Received(1).SaveAsync();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldReturnArgumentException_IfTodoItemIsNull()
        {
            // Act
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _service.CreateAsync(null));

            // Assert
            _repository.DidNotReceive().Create(default);
            await _repository.DidNotReceive().SaveAsync();
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldUpdateItem_IfExists()
        {
            //Arrange
            var existedItem = new Fixture().Create<TodoItem>();
            var newItem = new Fixture().Build<TodoItemDTO>().With(i => i.Id, existedItem.Id).Create();
            _repository.GetAsync(existedItem.Id).Returns(existedItem);

            // Act
            await _service.UpdateAsync(existedItem.Id, newItem);

            //Assert

            await _repository.Received(1).GetAsync(existedItem.Id);
            _repository.Received(1).Update(Arg.Any<TodoItem>());
            await _repository.Received(1).SaveAsync();
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldReturnNotFoundException_IfTodoItemNotExists()
        {
            //Arrange
            var existedItem = new Fixture().Create<TodoItem>();
            var newItem = new Fixture().Build<TodoItemDTO>().With(i => i.Id, existedItem.Id + 1).Create();
            _repository.GetAsync(newItem.Id).Returns<TodoItem>(l => null);

            // Act
            await Assert.ThrowsExceptionAsync<NotFoundException>(async () => await _service.UpdateAsync(newItem.Id, newItem));

            // Assert
            await _repository.Received(1).GetAsync(newItem.Id);
            _repository.DidNotReceive().Update(Arg.Any<TodoItem>());
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public async Task UpdateAsync_ShouldReturnArgumentException_IfIdIncorrect(long id)
        {
            // Arrange
            var item = new Fixture().Create<TodoItemDTO>();

            //Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _service.UpdateAsync(id, item));

            // Assert
            await _repository.DidNotReceive().GetAsync(item.Id);
            await _repository.DidNotReceive().SaveAsync();
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldReturnArgumentException_IfIdIncorrect()
        {
            //Arrange
            var id = new Fixture().Create<long>();

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _service.UpdateAsync(id, null));

            //Assert
            await _repository.DidNotReceive().GetAsync(id);
            await _repository.DidNotReceive().SaveAsync();
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public async Task DeleteAsync_ShouldReturnArgumentException_IfIdIncorrect(long id)
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _service.DeleteAsync(id));

            //Assert
            _repository.DidNotReceive().Delete(Arg.Any<TodoItem>());
            await _repository.DidNotReceive().SaveAsync();
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldReturnNotFoundException_IfTodoItemNotExists()
        {
            //Arrange
            var item = new Fixture().Create<TodoItem>();
            _repository.GetAsync(item.Id).Returns<TodoItem>(l => null);

            // Act
            await Assert.ThrowsExceptionAsync<NotFoundException>(async () => await _service.DeleteAsync(item.Id));

            // Assert
            await _repository.Received(1).GetAsync(item.Id);
            _repository.DidNotReceive().Delete(item);
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldReturnCompletedTask_IfOK()
        {
            //Arrange
            var item = new Fixture().Create<TodoItem>();
            _repository.GetAsync(item.Id).Returns(item);

            // Act
            await _service.DeleteAsync(item.Id);

            //Assert
            _repository.Received(1).Delete(item);
            await _repository.Received(1).SaveAsync();
        }
    }
}
