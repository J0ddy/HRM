﻿using AutoMapper;
using Data.Model;
using HotelReservationsManager.Model;
using Web.Models.Clients;
using Web.Models.InputModels;
using Web.Models.Reservations;
using Web.Models.Rooms;
using Web.Models.ViewModels;

namespace Web.Common
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Reservations 
            CreateMap<Reservation, ReservationViewModel>(); 
            CreateMap<ReservationInputModel, Reservation>();
            CreateMap<Reservation, ReservationPeriod>();
            CreateMap<ReservationViewModel, ReservationPeriod>();
            CreateMap<Reservation, ReservationInputModel>();

            // Clients
            CreateMap<ClientViewModel, ClientData>();
            CreateMap<ClientData, ClientViewModel>();
            CreateMap<ClientData, ClientInputModel>();
            CreateMap<ClientInputModel, ClientData>();
            
            // Users
            CreateMap<ApplicationUser, UserDataViewModel>();

            // Employees
            CreateMap<EmployeeData, EmployeeDataViewModel>();
            CreateMap<EmployeeData, EmployeeDataViewModel>();
            CreateMap<EmployeeData, EmployeeInputModel>();
            CreateMap<ApplicationUser, EmployeeDataViewModel>();
            CreateMap<ApplicationUser, EmployeeInputModel>();
            CreateMap<EmployeeData, ApplicationUser>();
            CreateMap<EmployeeInputModel, EmployeeData>();
            CreateMap<EmployeeInputModel, ApplicationUser>();

            // Rooms
            CreateMap<Room, RoomViewModel>();
            CreateMap<Room, RoomInputModel>();
        }
    }
}
