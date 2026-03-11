using AutoMapper;
using BusinessLayer.Concrete;
using DataAccessLayer.Abstract;
using EntityLayer.Constants;
using EntityLayer.DTOs.Company;
using EntityLayer.Entities.Domain;
using EntityLayer.Exceptions;
using FluentAssertions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FinanceApi.Tests.CompanyTest
{
    public class CompanyServiceTests
    {
        private readonly Mock<IGenericRepository<Company>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CompanyService _companyService;

        public CompanyServiceTests()
        {
            _mockRepo = new Mock<IGenericRepository<Company>>();
            _mockMapper = new Mock<IMapper>();
            _companyService = new CompanyService(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllCompaniesAsync_Succesfull()
        {
            int userId = 1;
            var filter = new CompanyFilterDto { Name = "Test", PageNumber = 1, PageSize = 10 };
            var companies = new List<Company>
            {
                new Company { Id = 1, Name = "Test Company 1", Address = "Addr 1", UserId = userId, CreateDate = DateTime.Now },
                new Company { Id = 2, Name = "Other Company", Address = "Addr 2", UserId = userId, CreateDate = DateTime.Now },
                new Company { Id = 3, Name = "Test Company 2", Address = "Addr 3", UserId = 2, CreateDate = DateTime.Now }
            };

            var mockQuery = companies.BuildMock();
            _mockRepo.Setup(x => x.GetQueryable()).Returns(mockQuery);

            var mappedDtos = new List<CompanyListDto> { new CompanyListDto { Name = "Test Company 1" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<CompanyListDto>>(It.IsAny<IEnumerable<Company>>())).Returns(mappedDtos);

            var result = await _companyService.GetAllCompaniesAsync(filter, userId);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCompanyExists()
        {
            var company = new Company { Id = 1, Name = "Test" };
            var dto = new CompanyListDto { Id = 1, Name = "Test" };

            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(company);
            _mockMapper.Setup(m => m.Map<CompanyListDto>(company)).Returns(dto);

            var result = await _companyService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result.Name.Should().Be("Test");
        }

        [Fact]
        public async Task GetByIdAsync_WhenCompanyDoesNotExist()
        {
            _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Company)null);

            Func<Task> action = async () => await _companyService.GetByIdAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CompanyNotFound);
        }

        [Fact]
        public async Task AddAsync_WhenTaxNumberExists()
        {
            var dto = new CreateCompanyDto { TaxNumber = "12345" };
            _mockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Company, bool>>>())).ReturnsAsync(true);

            Func<Task> action = async () => await _companyService.AddAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CompanyAlreadyExists);
        }

        [Fact]
        public async Task AddAsync_WhenSuccessful()
        {
            var dto = new CreateCompanyDto { Name = "New Co", TaxNumber = "123" };
            var company = new Company { Name = "New Co", TaxNumber = "123" };

            _mockRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Company, bool>>>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<Company>(dto)).Returns(company);

            await _companyService.AddAsync(dto);

            _mockRepo.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCompanyExists_ShouldUpdate()
        {
            var dto = new UpdateCompanyDto { Id = 1, Name = "Updated" };
            var existingCompany = new Company { Id = 1, Name = "Old" };

            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingCompany);

            await _companyService.UpdateAsync(dto);

            _mockMapper.Verify(m => m.Map(dto, existingCompany), Times.Once);
            _mockRepo.Verify(x => x.Update(existingCompany), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCompanyDoesNotExist()
        {
            var dto = new UpdateCompanyDto { Id = 1 };
            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Company)null);

            Func<Task> action = async () => await _companyService.UpdateAsync(dto);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CompanyNotFound);
        }

        [Fact]
        public async Task DeleteAsync_WhenCompanyExists_ShouldDelete()
        {
            var company = new Company { Id = 1 };
            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(company);

            await _companyService.DeleteAsync(1);

            _mockRepo.Verify(x => x.Delete(company), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenCompanyDoesNotExist_ShouldThrowBusinessException()
        {
            _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Company)null);

            Func<Task> action = async () => await _companyService.DeleteAsync(1);

            await action.Should().ThrowAsync<BusinessException>()
                .Where(x => x.Message == ErrorKeys.CompanyNotFound);
        }
    }
}