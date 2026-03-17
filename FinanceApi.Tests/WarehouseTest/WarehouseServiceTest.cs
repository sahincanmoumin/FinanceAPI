using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.DTOs.Warehouse;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Enums;
using EntityLayer.Exceptions;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BusinessLayer.Tests
{
    public class WarehouseServiceTests
    {
        private readonly Mock<IGenericRepository<Warehouse>> _warehouseRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly WarehouseService _warehouseService;

        public WarehouseServiceTests()
        {
            _warehouseRepoMock = new Mock<IGenericRepository<Warehouse>>();
            _mapperMock = new Mock<IMapper>();
            _warehouseService = new WarehouseService(_warehouseRepoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenWarehouseCodeAlreadyExists()
        {
            var dto = new CreateWarehouseDto { Code = "W01", CompanyId = 1, Type = WarehouseType.Branch };
            _warehouseRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessException>(() => _warehouseService.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_WhenSecondMainWarehouseAdded_ShouldThrowException()
        {
            var dto = new CreateWarehouseDto
            {
                Code = "W02",
                CompanyId = 1,
                Type = WarehouseType.Main
            };

            _warehouseRepoMock.SetupSequence(x => x.AnyAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(false)
                .ReturnsAsync(true);

            await _warehouseService.Invoking(s => s.AddAsync(dto))
                .Should().ThrowAsync<BusinessException>();

            _warehouseRepoMock.Verify(x => x.AnyAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AddAsync_WhenSuccess()
        {
            var dto = new CreateWarehouseDto { Code = "W03", CompanyId = 1, Type = WarehouseType.Branch };
            var entity = new Warehouse { Id = 1, Code = "W03", CompanyId = 1 };

            _warehouseRepoMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<Warehouse>(dto)).Returns(entity);
            _mapperMock.Setup(m => m.Map<WarehouseListDto>(entity)).Returns(new WarehouseListDto { Id = 1, Code = "W03" });

            var result = await _warehouseService.AddAsync(dto);

            Assert.NotNull(result);
            _warehouseRepoMock.Verify(x => x.AddAsync(It.IsAny<Warehouse>()), Times.Once);
        }
    }
}