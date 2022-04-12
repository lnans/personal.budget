global using Api.Installers;
global using Api.Middlewares;
global using Application.Commands.Accounts;
global using Application.Commands.OperationTags;
global using Application.Common.Helpers;
global using Application.Queries.Auth;
global using Application.Queries.OperationTags;
global using Application;
global using Domain.Common;
global using Domain.Entities;
global using Domain.Exceptions;
global using Domain;
global using Infrastructure.Persistence;
global using Infrastructure;
global using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
global using MediatR;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using System.Net;
global using System.Security.Authentication;
global using System.Text.Json.Serialization;
global using System.Text.Json;
global using System.Text;