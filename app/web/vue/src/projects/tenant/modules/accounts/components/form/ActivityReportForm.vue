<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.sendActivityReport')"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 500px"
  >
    <div class="d-flex flex-column justify-content-center align-items-center">
      <div class="w-100">
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-width="120px"
          status-icon
        >
          <el-form-item :label="$t('fields.startDate')" prop="from">
            <el-date-picker
              v-model="formData.from"
              type="datetime"
              placeholder="Pick a start day"
            />
          </el-form-item>

          <el-form-item :label="$t('fields.endDate')" prop="to">
            <el-date-picker
              v-model="formData.to"
              type="datetime"
              placeholder="Pick a end day"
            />
          </el-form-item>

          <el-form-item :label="$t('fields.receiverEmail')" prop="email">
            <el-input v-model="formData.email" />
          </el-form-item>

          <!-- <el-form-item label="BCC" prop="bcc">
            <el-select
              v-model="formData.bcc"
              multiple
              filterable
              allow-create
              default-first-option
              :reserve-keyword="false"
              placeholder="Forward this report"
            >
            </el-select>
          </el-form-item> -->
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import type { FormInstance } from "element-plus";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import { useStore } from "@/store";
import moment from "moment";

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();

const store = useStore();
const _email = store.state.AuthModule?.user?.email;
const id = ref(0);

const ruleFormRef = ref<FormInstance>();

const validateFromIsLessThanTo = (rule, value, callback) => {
  if (formData.value.from >= formData.value.to) {
    callback(new Error("End Date must larger than Start Date"));
  } else {
    callback();
  }
};

const rules = reactive<any>({
  email: [
    {
      required: true,
      message: "Please input from",
      trigger: "blur",
    },
  ],

  from: [
    {
      required: true,
      message: "Please input from",
      trigger: "blur",
    },
  ],
  to: [
    {
      required: true,
      message: "Please input to",
      trigger: "blur",
    },
    { validator: validateFromIsLessThanTo, trigger: "blur" },
  ],
});

const formData = ref<any>({
  id: id.value,
  email: _email,
  from: "",
  to: "",
  bcc: [],
});

const submit = async () => {
  isLoading.value = true;
  formData.value.id = id.value;
  console.log(formData.value);
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) {
    isLoading.value = false;
    return;
  }

  try {
    processDate();
    await AccountService.getActivityReport(
      formData.value.id,
      formData.value
    ).then(MsgPrompt.success("success"));
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

const processDate = () => {
  formData.value.from = moment(formData.value.from).local().toISOString();
  formData.value.to = moment(formData.value.to).local().toISOString();
};

const show = async (_id: number) => {
  isLoading.value = true;
  id.value = _id;
  modalRef.value?.show();
  resetForm();

  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const resetForm = () => {
  if (!ruleFormRef.value) return;
  ruleFormRef.value.resetFields();
};

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

defineExpose({
  show,
  hide,
});
</script>

<style scope lang="scss">
.el-select {
  width: 100%;
}
// .el-select-dropdown {
//   display: none;
// }
.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}
</style>
