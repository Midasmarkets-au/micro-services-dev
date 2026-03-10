<template>
  <div v-if="isLoading">
    <LoadingRing />
  </div>
  <div v-else-if="!isLoading && policyList.length === 0">
    <NoDataBox />
  </div>
  <div v-else class="fw-semibold text-gray-900">
    <p class="fs-4">{{ $t("title.updatePaymentServicePolicy") }}</p>
    <div class="input-group w-md-25 mb-4">
      <select class="form-select" v-model="langItem" :disabled="!canAddLang">
        <option
          v-for="item in langOptions"
          :key="item.code"
          :value="item.code"
          :label="item.code + '   (' + item.name + ')'"
        />
      </select>
      <button class="btn btn-success" @click="addLang" :disabled="!canAddLang">
        {{ $t("action.addALanguage") }}
      </button>
    </div>
    <el-tabs>
      <el-tab-pane
        :label="index"
        v-for="(item, index) in policyList"
        :key="index"
      >
        <QuillEditor
          theme="snow"
          v-model:content="policyList[index]"
          contentType="html"
          :modules="modules"
          toolbar="full"
        />
      </el-tab-pane>
    </el-tabs>
    <div class="d-flex justify-content-end gap-10 mt-4">
      <button class="btn btn-secondary" @click="close">
        {{ $t("action.close") }}
      </button>
      <button class="btn btn-primary" @click="save">
        {{ $t("action.save") }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, defineProps, onMounted, computed } from "vue";
import PaymentService from "../services/PaymentService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const props = defineProps({
  paymentServiceId: {
    type: Number,
    required: true,
  },
  closeFunction: Function,
});

const uploadImg = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await PaymentService.uploadImageV2(formData);
  return res["url"];
};
const isLoading = ref(true);
const policyList = ref(Array<any>());
const langOptions = ref([]);
const langItem = ref("");

const fetchData = async () => {
  isLoading.value = true;
  const paymentId = computed(() => props.paymentServiceId);

  try {
    const responseBody = await PaymentService.getPaymentMethodPolicyById(
      paymentId.value
    );
    policyList.value = responseBody;

    populateLangOptions();
  } catch (error) {
    console.log(error);
  }

  isLoading.value = false;
};

const save = async () => {
  isLoading.value = true;
  try {
    await PaymentService.updatePaymentMethodPolicyById(
      props.paymentServiceId,
      policyList.value
    );
    MsgPrompt.success("Save successfully");
  } catch (error) {
    MsgPrompt.error("Save failed");
  }
  isLoading.value = false;
};

const close = () => {
  props.closeFunction();
};

const addLang = () => {
  if (Object.keys(policyList.value).length < LanguageTypes.all.length) {
    policyList.value[langItem.value] = "content";
    populateLangOptions();
  }
};

const canAddLang = computed(() => {
  return Object.keys(policyList.value).length < LanguageTypes.all.length;
});

const populateLangOptions = () => {
  const instructionKeys = Object.keys(policyList.value);
  langOptions.value = LanguageTypes.all.filter(
    (lang) => !instructionKeys.includes(lang.code)
  );
  if (langOptions.value.length > 0) {
    langItem.value = langOptions.value[0].code;
  }
};

const modules = [
  {
    name: "imageUploader",
    module: ImageUploader,
    options: {
      upload: (file) => {
        return uploadImg(file);
      },
    },
  },
  {
    name: "blotFormatter",
    module: BlotFormatter,
  },
];

onMounted(async () => {
  await fetchData();

  if (langOptions.value.length > 0) {
    langItem.value = langOptions.value[0].code;
  }
});
</script>
<style scoped>
:deep .ql-container {
  min-height: 350px;
}
</style>
