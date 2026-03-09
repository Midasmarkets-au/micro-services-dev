<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="ChecklistCreateRef"
  >
    <div class="modal-dialog modal-dialog-centered" style="min-width: 950px">
      <div class="modal-content px-10 py-10">
        <div class="border-bottom mb-10 pb-5">
          <div class="d-flex justify-content-between">
            <p class="fs-1">{{ $t("action.updateOffsetCheck") }}</p>
          </div>
        </div>
        <div class="pt-5">
          <el-form
            :model="formData"
            :rules="formRule"
            ref="ruleFormRef"
            label-width="120px"
          >
            <el-form-item label="Name" prop="name">
              <el-input
                v-model="formData.name"
                placeholder="Name"
                clearable
              ></el-input>
            </el-form-item>

            <el-form-item label="Members" prop="accountNumbers" class="mt-5">
              <el-input
                v-model="formData.accountNumbers"
                placeholder="Separate by Comma"
                clearable
              ></el-input>
            </el-form-item>

            <div class="text-end mt-15">
              <el-button
                type="success"
                :disable="isLoading"
                @click="updateChecklist(ruleFormRef)"
              >
                {{ $t("action.submit") }}
              </el-button>
              <el-button type="danger" @click.prevent="close">
                {{ $t("action.close") }}
              </el-button>
            </div>
          </el-form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, reactive } from "vue";
import { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TradeServices from "../services/TradeServices";
import { hideModal, showModal } from "@/core/helpers/dom";

const siteId = ref(0);
const { t } = useI18n();
const detail = ref<any>({});
const isLoading = ref(false);
const ChecklistCreateRef = ref(null);
const ruleFormRef = ref<FormInstance>();

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const formData = ref<any>({
  name: detail.value.name,
  accountNumbers: detail.value.accountNumbers,
  type: detail.value.type,
  status: detail.value.status,
});

const formRule = reactive<any>({
  name: [{ required: true, message: "Please input name", trigger: "blur" }],
  accountNumbers: [
    { required: true, message: "Please input members", trigger: "blur" },
  ],
});

const updateChecklist = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;

  await formEl?.validate();

  formData.value.type = detail.value.type;
  formData.value.status = detail.value.status;

  try {
    if (typeof formData.value.accountNumbers === "string") {
      formData.value.accountNumbers = formData.value.accountNumbers
        .split(",")
        .map(Number);
    }
    await TradeServices.updateChecklist(detail.value.id, formData.value);
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("eventSubmit");
      close();
    });
  } catch (error) {
    console.log(error);
  }

  isLoading.value = false;
};

const show = (data, _siteId) => {
  siteId.value = _siteId;
  detail.value = data;
  formData.value = {
    name: detail.value.name,
    accountNumbers: detail.value.accountNumbers,
  };
  showModal(ChecklistCreateRef.value);
};

const close = () => {
  hideModal(ChecklistCreateRef.value);
};

defineExpose({ show });
</script>
