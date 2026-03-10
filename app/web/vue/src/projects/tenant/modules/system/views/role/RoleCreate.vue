<template>
  <ModelForm
    :submit="submit"
    :discard="close"
    :title="$t('action.create')"
    elId="roleCreate"
    :isLoading="isLoading"
    :submited="submited"
    :savingTitle="$t('action.saving')"
    ref="refRoleCreateModel"
    :rules="rules"
    :formData="formData"
  >
    <div>
      <div class="fv-row mb-7">
        <label class="required fs-6 fw-semobold mb-2">
          {{ $t("fields.name") }}
        </label>
        <el-form-item prop="name">
          <el-input
            v-model="formData.name"
            name="name"
            type="text"
            placeholder=""
            style=""
          />
        </el-form-item>
      </div>
      <PermissionList ref="permissionList" />
    </div>
  </ModelForm>
</template>
<script lang="ts" setup>
// import ApiService from "@/core/services/ApiService";
import { ref, inject } from "vue";
import ModelForm from "@/components/ModelForm.vue";
import ErrorMsg from "@/components/ErrorMsg";
import PermissionList from "../permission/PermissionView.vue";
import { apiProviderKey } from "@/core/plugins/providerKeys";

const api = inject(apiProviderKey);

const refRoleCreateModel = ref(null);
const props = defineProps({
  permissions: { type: Array, required: true },
});

const selectPermissions = ref([]);

const emit = defineEmits(["created"]);

const isLoading = ref(true);
const submited = ref(true);
const permissionList = ref(null);

// let checkFalg = false;

const formData = ref({
  name: "",
  permissions: {},
});

const rules = ref({
  name: [
    {
      required: true,
      message: "Role name is required",
      trigger: "change",
    },
  ],
  permissions: [
    {
      required: false,
      type: "array",
    },
  ],
});

const submit = async () => {
  isLoading.value = submited.value = true;
  await api["role.create"]({
    data: {
      name: formData.value.name,
      permissions: selectPermissions.value,
    },
  })
    .then(({ data }) => {
      isLoading.value = submited.value = false;
      emit("created", data.data);
      refRoleCreateModel.value.hide();
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      isLoading.value = submited.value = false;
    });
};

const close = () => {
  console.log("close");
};

const show = () => {
  refRoleCreateModel.value.show();
  resetForm();
  permissionList.value.initData(selectPermissions.value, props.permissions);
  isLoading.value = false;
  submited.value = false;
};

const resetForm = () => {
  formData.value = {
    name: "",
    permissions: {},
  };
};
defineExpose({
  resetForm,
  show,
});
</script>
