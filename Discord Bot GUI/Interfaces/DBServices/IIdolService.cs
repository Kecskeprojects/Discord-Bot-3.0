﻿using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IIdolService
{
    Task<DbProcessResultEnum> AddIdolAsync(string idolName, string idolGroup);
    Task<List<IdolResource>> GetIdolsByGroupAsync(string groupName);
    Task<DbProcessResultEnum> RemoveIdolAsync(string idolName, string idolGroup);
    Task<List<IdolResource>> GetAllIdolsAsync();
    Task<bool> UpdateIdolDetailsAsync(IdolResource idolResource, ExtendedBiasData data, AdditionalIdolData additional);
    Task<IdolExtendedResource> GetIdolDetailsAsync(string idolName, string idolGroup);
    Task<DbProcessResultEnum> UpdateAsync(int idolId, EditIdolModal modal);
    Task<DbProcessResultEnum> UpdateAsync(int idolId, EditIdolExtendedModal modal);
    Task<DbProcessResultEnum> UpdateAsync(int idolId, ChangeIdolProfileLinkModal modal);
    Task<DbProcessResultEnum> UpdateAsync(int idolId, ChangeIdolGroupModal modal);
    Task<ListWithDbResult<UserResource>> GetUsersByIdolsAsync(string[] nameList);
    Task<List<IdolGameResource>> GetListForGameAsync(GenderEnum gender, int debutAfter, int debutBefore);
    Task<int> CorrectUpdateErrorsAsync();
    Task<IdolGameResource> GetIdolByIdAsync(int idolId);
}
