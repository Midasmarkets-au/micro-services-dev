<template>
  <div class="card w-75 p-5">
    <el-form label-width="100px">
      <div class="row">
        <div class="col-4">
          <el-form-item label="Party Id">
            {{ userInfos.partyId }}
          </el-form-item>
        </div>
        <div class="col-4">
          <el-form-item label="Party UID">
            {{ userInfos.uid }}
          </el-form-item>
        </div>
        <div class="col-4">
          <!-- <el-form-item :label="t('fields.language')">
            {{ userInfos.language }}
          </el-form-item> -->
        </div>
      </div>
    </el-form>
    <el-form label-width="100px">
      <div class="row">
        <div class="col-4">
          <el-form-item :label="t('fields.createdOn')">
            <TimeShow :dateIsoString="userInfos.createdOn" type="withoutSec" />
          </el-form-item>
        </div>
        <div class="col-4">
          <el-form-item :label="t('fields.updatedOn')">
            <TimeShow :dateIsoString="userInfos.updatedOn" type="withoutSec" />
          </el-form-item>
        </div>
        <div class="col-4">
          <!-- <el-form-item :label="t('fields.language')">
            {{ userInfos.language }}
          </el-form-item> -->
        </div>
      </div>
    </el-form>
    <el-form label-width="100px">
      <div class="row">
        <div class="col-4">
          <el-form-item :label="t('fields.lastLoginIp')">
            {{ userInfos.lastLoginIp }}
          </el-form-item>
        </div>
        <div class="col-4">
          <el-form-item :label="t('fields.registeredIp')">
            {{ userInfos.registeredIp }}
          </el-form-item>
        </div>
        <div class="col-4">
          <el-form-item :label="t('fields.language')">
            {{ userInfos.language }}
          </el-form-item>
        </div>
      </div>
    </el-form>

    <el-form label-width="100px">
      <div v-if="formData.socialMedium">
        <el-divider />
        <div class="head">social Media</div>
        <div v-for="(type, index) in formData.socialMedium" :key="index">
          <div v-if="type.account != ''" class="d-flex justify-content-between">
            <el-form-item
              :label="$t('fields.' + type.name)"
              :prop="type.name"
              style="width: 90%"
            >
              <el-input v-model="type.account" disabled></el-input>
            </el-form-item>

            <CopyButton class="ms-3" :content="type.account" />
          </div>
        </div>
        <hr class="mb-7" />
      </div>
      <div class="row">
        <div class="col-6">
          <div class="d-flex justify-content-between">
            <el-form-item
              :label="t('fields.siteId')"
              prop="siteID"
              style="width: 90%"
            >
              <el-input v-model="showSiteId" disabled></el-input>
            </el-form-item>

            <el-button
              class="ms-3"
              type="primary"
              @click="changeSiteModalRef?.show(partyId, siteId)"
              >{{ $t("action.update") }}</el-button
            >
          </div>
        </div>
        <div class="col-6">
          <div class="d-flex justify-content-between">
            <el-form-item :label="t('fields.status')">
              <el-switch
                v-model="userInfos.status"
                active-color="#13ce66"
                inactive-color="#ff4949"
                :active-text="t('status.active')"
                :inactive-text="t('status.inactive')"
                :active-value="PartyStatusTypes.Active"
                :inactive-value="PartyStatusTypes.Closed"
                disabled
              />
            </el-form-item>
            <el-button
              class="ms-3"
              type="primary"
              @click="updateUserStatusRef?.show()"
              >{{ $t("action.update") }}</el-button
            >
          </div>
        </div>
      </div>
    </el-form>
  </div>
  <div class="card w-75 p-10 mt-5">
    <div class="head">Basic Info</div>
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="100px"
    >
      <div class="row">
        <div class="col-6">
          <el-form-item :label="t('fields.email')" prop="email">
            <el-input v-model="formData.email"></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="t('fields.gender')" prop="gender">
            <el-radio-group v-model="formData.gender">
              <el-radio :label="0">{{ t("fields.male") }}</el-radio>
              <el-radio :label="1">{{ t("fields.female") }}</el-radio>
            </el-radio-group>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-6">
          <el-form-item :label="t('fields.firstName')" prop="firstName">
            <el-input v-model="formData.firstName"></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="t('fields.lastName')" prop="lastName">
            <el-input v-model="formData.lastName"></el-input>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-6">
          <el-form-item :label="t('fields.nativeName')" prop="nativeName">
            <el-input v-model="formData.nativeName"></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="t('fields.birthdate')" prop="birthday">
            <el-date-picker
              v-model="formData.birthday"
              type="date"
              value-format="YYYY-MM-DD"
            ></el-date-picker>
          </el-form-item>
        </div>
      </div>

      <div class="row">
        <div class="col-3">
          <el-form-item
            :label-width="100"
            :label="t('fields.citizen')"
            prop="citizen"
          >
            <el-input v-model="formData.citizen"></el-input>
          </el-form-item>
        </div>
        <div class="col-3">
          <el-form-item :label-width="100" label="ccc" prop="ccc">
            <el-input v-model="formData.ccc"></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item
            :label-width="100"
            :label="t('fields.phone')"
            prop="phoneNumber"
          >
            <el-input v-model="formData.phoneNumber"></el-input>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-8">
          <el-form-item :label="t('fields.address')" prop="address">
            <el-input v-model="formData.address"></el-input>
          </el-form-item>
        </div>
      </div>

      <el-divider />
      <div class="head">ID Info</div>

      <el-form-item :label="t('fields.idType')" prop="idType">
        <el-radio-group v-model="formData.idType">
          <el-radio :label="1"> {{ $t("fields.govId") }}</el-radio>
          <el-radio :label="2"> {{ $t("fields.driveLicense") }}</el-radio>
          <el-radio :label="3"> {{ $t("fields.passport") }}</el-radio>
        </el-radio-group>
      </el-form-item>
      <div class="row">
        <div class="col-6">
          <el-form-item :label="t('fields.idNumber')" prop="idNumber">
            <el-input v-model="formData.idNumber"></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="t('fields.idIssuer')" prop="idIssuer">
            <el-input v-model="formData.idIssuer"></el-input>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-6">
          <el-form-item :label="t('fields.idIssuedOn')" prop="idIssuedOn">
            <el-date-picker
              v-model="formData.idIssuedOn"
              type="date"
              value-format="YYYY-MM-DD"
            ></el-date-picker>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="t('fields.idExpiresOn')" prop="idExpireOn">
            <el-date-picker
              v-model="formData.idExpireOn"
              type="date"
              value-format="YYYY-MM-DD"
            ></el-date-picker>
          </el-form-item>
        </div>
      </div>
      <!-- <el-form-item label="Roles" prop="roles">
        <UserPermissions :userDetails="props.userDetails" />
      </el-form-item> -->
      <el-divider />
      <div class="row">
        <div class="col-4">
          <el-form-item :label-width="100" label="Refer Code" prop="Refer Code">
            <el-input v-model="formData.referCode"></el-input>
          </el-form-item>
        </div>
      </div>
      <el-form-item>
        <el-button type="primary" @click="submitForm" :loading="isLoading">{{
          $t("action.submit")
        }}</el-button>
        <el-button :disabled="isLoading" @click="resetForm()">{{
          $t("action.reset")
        }}</el-button>
      </el-form-item>
    </el-form>

    <ChangeSiteModal
      ref="changeSiteModalRef"
      @updateSiteID="updateSiteID"
    ></ChangeSiteModal>
    <UpdateUserStatus ref="updateUserStatusRef"></UpdateUserStatus>
  </div>
  <div class="card w-75 p-10 mt-5">
    <div class="head">Options</div>
    <TagsView type="party" :id="partyId" />
  </div>
</template>

<script lang="ts" setup>
import { onMounted, reactive, ref, inject } from "vue";
import type { FormInstance, FormRules } from "element-plus";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import ChangeSiteModal from "./modal/ChangeSiteModal.vue";
import UpdateUserStatus from "./modal/UpdateUserStatus.vue";
import { useI18n } from "vue-i18n";
import { PartyStatusTypes } from "@/core/types/PartyStatusTypes";
import CopyButton from "@/components/CopyButton.vue";
import notification from "@/core/plugins/notification";
import { TagNames } from "@/core/types/TagTypes";
import TagsView from "@/projects/tenant/components/TagsView.vue";
import TimeShow from "@/components/TimeShow.vue";

const { t } = useI18n();
const changeSiteModalRef = ref<InstanceType<typeof ChangeSiteModal>>();
const updateUserStatusRef = ref<InstanceType<typeof UpdateUserStatus>>();
const showSiteId = ref("");
const ruleFormRef = ref<FormInstance>();
const formData = ref<Record<string, any>>({});
const isLoading = ref(false);

const userInfos = inject<any>("userInfos");
const checkedTags = ref<any>(userInfos.value.tags ?? []);

const tags = ref<any>([TagNames.Special]);
const partyId = inject<number>("partyId");
const rules = reactive<FormRules>({
  email: [
    { required: true, message: "Please input email", trigger: "blur" },
    { type: "email", message: "Please input correct email", trigger: "blur" },
  ],
  firstName: [
    { required: true, message: "Please input first name", trigger: "blur" },
  ],
  lastName: [
    { required: true, message: "Please input last name", trigger: "blur" },
  ],
});

const submitForm = async () => {
  if (!ruleFormRef.value) return;
  await ruleFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        isLoading.value = true;
        const res = await TenantGlobalService.updateUserProfileInfo(
          partyId.value,
          formData.value
        );
        notification.success();
        await resetForm(res);
        userInfos.value = res;
      } catch (error) {
        notification.danger();
      } finally {
        isLoading.value = false;
      }
    } else {
      notification.danger();
    }
  });
};

const updateTag = async (tag: any) => {
  isLoading.value = true;
  try {
    console.log("checkedTags", checkedTags.value);
    console.log("tag", tag);
    if (checkedTags.value.includes(tag)) {
      await UserService.updateUserTag(partyId.value, tag);
    } else {
      await UserService.deleteUserTag(partyId.value, tag);
    }
    notification.success();
  } catch (error) {
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const updateStatus = async (newVal: any) => {
  try {
    isLoading.value = true;
    let form = {
      status: newVal,
    };
    await UserService.updateUserStatus(partyId.value, form);
    notification.success();
  } catch (error) {
    notification.danger();
  } finally {
    isLoading.value = false;
  }
};

const siteId = ref(userInfos.value.siteId ?? 0);

const updateSiteID = async (_siteId: number) => {
  showSiteId.value = t("type.siteType." + _siteId);
};

const resetForm = async (_initialFormData?: any) => {
  if (!ruleFormRef.value) return;
  ruleFormRef.value.resetFields();
  const initialForm = _initialFormData ?? userInfos.value;
  formData.value = {
    ...initialForm,
    gender: parseInt(initialForm.gender),
  };
};

onMounted(() => {
  resetForm();

  showSiteId.value = t("type.siteType." + userInfos.value.siteId);
});
</script>

<style scoped>
.socialMediaHead {
  width: 5px;
  height: 22px;
  margin-right: 10px;
  background-color: #ffd400;
}
.head {
  font-weight: bold;
  font-size: 14px;
  padding-bottom: 2px;
}
</style>
