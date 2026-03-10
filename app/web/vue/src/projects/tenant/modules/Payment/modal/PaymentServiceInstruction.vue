<template>
  <div v-if="isLoading">
    <LoadingRing />
  </div>
  <div v-else-if="!isLoading && instructionList.length === 0">
    <NoDataBox />
  </div>
  <div v-else class="fw-semibold text-gray-900">
    <p class="fs-4">{{ $t("title.updatePaymentServiceInstruction") }}</p>
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
        v-for="(item, index) in instructionList"
        :key="index"
      >
        <div>
          <quill-editor
            :key="index"
            theme="snow"
            v-model:content="instructionList[index]"
            contentType="html"
            :modules="modules"
            toolbar="full"
          />
        </div>
      </el-tab-pane>
    </el-tabs>

    <div class="d-flex justify-content-end gap-10 mt-4">
      <button class="btn btn-secondary" @click.prevent="close">
        {{ $t("action.close") }}
      </button>
      <button class="btn btn-primary" @click.prevent="save">
        {{ $t("action.save") }}
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, defineProps, onMounted, computed } from "vue";
import PaymentService from "../services/PaymentService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { QuillEditor } from "@vueup/vue-quill";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import ImageUploader from "quill-image-uploader";
import BlotFormatter from "quill-blot-formatter/dist/BlotFormatter";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const props = defineProps({
  paymentServiceId: {
    type: Number,
    required: true,
  },
  closeFunction: Function,
});

const isLoading = ref(true);
const instructionList = ref(Array<any>());
const langOptions = ref<any>([]);
const langItem = ref<any>("");

const uploadImg = async (file: any) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await PaymentService.uploadImage(formData);
  return res["url"];
};

const fetchData = async () => {
  isLoading.value = true;
  const paymentId = computed(() => props.paymentServiceId);

  try {
    const responseBody = await PaymentService.getPaymentMethodInstructionById(
      paymentId.value
    );
    instructionList.value = responseBody;

    populateLangOptions();
  } catch (error) {
    console.log(error);
  }

  isLoading.value = false;
};

const save = async () => {
  isLoading.value = true;
  try {
    await PaymentService.updatePaymentMethodInstructionById(
      props.paymentServiceId,
      instructionList.value
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
  if (Object.keys(instructionList.value).length < LanguageTypes.all.length) {
    instructionList.value[langItem.value] = "content";
    populateLangOptions();
  }
};

const canAddLang = computed(() => {
  return Object.keys(instructionList.value).length < LanguageTypes.all.length;
});

const populateLangOptions = () => {
  const instructionKeys = Object.keys(instructionList.value);
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
