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
            <p class="fs-1">{{ $t("action.addOffsetCheck") }}</p>
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
                type="textarea"
                clearable
              ></el-input>
            </el-form-item>

            <div class="text-end mt-15">
              <el-button
                type="success"
                :disable="isLoading"
                @click="addChecklist(ruleFormRef)"
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
const isLoading = ref(false);
const ChecklistCreateRef = ref(null);
const ruleFormRef = ref<FormInstance>();

const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();

const formData = ref<any>({
  name: "",
  accountNumbers: "",
});

const formRule = reactive<any>({
  name: [{ required: true, message: "Please input name", trigger: "blur" }],
  accountNumbers: [
    { required: true, message: "Please input members", trigger: "blur" },
  ],
});

const addChecklist = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;

  try {
    await formEl?.validate((valid) => {
      if (valid) {
        formData.value.accountNumbers = formData.value.accountNumbers
          .split(",")
          .map(Number);

        formData.value.type = 1;
        formData.value.status = 0;

        TradeServices.addChecklist(formData.value);
        MsgPrompt.success(t("tip.submitSuccess")).then(() => {
          emits("eventSubmit");
          clear();
          close();
        });
      } else {
        return false;
      }
    });
  } catch (error) {
    MsgPrompt.error(t("tip.submitError"));
  }
  isLoading.value = false;
};

const clear = () => {
  formData.value = {
    name: "",
    accountNumbers: "",
  };
};

const show = (_siteId) => {
  siteId.value = _siteId;
  showModal(ChecklistCreateRef.value);
};

const close = () => {
  hideModal(ChecklistCreateRef.value);
};

defineExpose({ show });
</script>
