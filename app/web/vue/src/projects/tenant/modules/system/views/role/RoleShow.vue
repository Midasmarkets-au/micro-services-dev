<template>
  <SiderDetail
    :save="submit"
    :discard="close"
    :title="title"
    elId="role_show"
    :isLoading="isLoading"
    :submited="submited"
    :isDisabled="formData.disabled"
    :savingTitle="$t('action.saving')"
    width="{default:'90%', 'md': '60%'}"
    ref="roleShowRef"
  >
    <div v-if="!isLoading">
      <div class="fv-row mb-7">
        <label class="required fs-6 fw-semobold mb-2">{{
          $t("fields.name")
        }}</label>
        <el-form-item prop="name">
          <el-input
            v-model="formData.name"
            name="name"
            type="text"
            placeholder=""
            :disabled="formData.disabled"
          />
        </el-form-item>
      </div>
      <PermissionList
        ref="roleShowPermissionList"
        :disabled="formData.disabled"
      />
    </div>
  </SiderDetail>
</template>

<script setup lang="ts">
import SiderDetail from "@/components/SiderDetail.vue";
import ErrorMsg from "@/components/ErrorMsg";
import PermissionList from "../permission/PermissionView.vue";
import { ref, nextTick, inject } from "vue";
import { useI18n } from "vue-i18n";
import { ElFormItem, ElInput } from "element-plus";
import { apiProviderKey } from "@/core/plugins/providerKeys";

const { t } = useI18n();

const api = inject(apiProviderKey);

const isLoading = ref(true);
const submited = ref(true);
const sourceData = ref({});
const roleShowRef = ref(null);
const roleShowPermissionList = ref(null);

const formData = ref({
  name: "",
  permissions: [],
});

const title = ref("");

const show = async (data, permissions) => {
  initData();
  title.value = t("action.edit");
  sourceData.value = data;
  formData.value = JSON.parse(JSON.stringify(data));
  isLoading.value = false;
  submited.value = false;
  await nextTick();
  roleShowPermissionList.value.initData(
    formData.value.permissions,
    permissions
  );
  // await nextTick();
  // roleShowRef.value.show();
};

const initData = () => {
  formData.value = {
    name: "",
  };
};

const submit = async () => {
  await api["role.update"]({
    id: formData.value.id,
    data: {
      id: formData.value.id,
      name: formData.value.name,
      permissions: formData.value.permissions,
    },
  })
    .then(({ data }) => {
      isLoading.value = submited.value = false;
      sourceData.value.name = data.data.name;
      sourceData.value.permissions = data.data.permissions;
      roleShowRef.value.hide();
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      isLoading.value = submited.value = false;
    });
};

const close = () => {
  initData();
};
defineExpose({ show });
</script>
