<template>
  <div class="d-flex gap-4">
    <el-button
      color="#626aef"
      :dark="true"
      size="small"
      @click="toggle = !toggle"
      >Translate</el-button
    >
    <div v-if="toggle">
      <el-button
        v-for="item in LanguageTypes.all"
        :key="item.code"
        size="small"
        @click="translate(item)"
        :disabled="isLoading"
      >
        {{ item.name }}
      </el-button>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, computed } from "vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import SupportService from "../services/SupportService";
import { ElNotification } from "element-plus";
const props = defineProps<{
  text: string;
  case: any;
}>();
const isLoading = ref(false);
const toggle = ref(false);
const translatedText = ref({
  content: "",
  language: "",
});
const id = computed(() => props.case.id);
// const content = computed(() => props.text);
const formData = ref<any>({
  targetLanguage: "English",
});
const emit = defineEmits(["translated"]);
const translate = async (language: any) => {
  isLoading.value = true;
  try {
    formData.value.targetLanguage = language.englishName;
    const res = await SupportService.translateCaseText(
      id.value,
      formData.value
    );
    translatedText.value.content = res;
    translatedText.value.language = language.name;
    ElNotification({
      title: language.name,
      message: "Translation successful",
      type: "success",
    });
    emit("translated");
  } catch (e) {
    ElNotification({
      title: language.name,
      message: "Translation failed",
      type: "error",
    });
    console.log(e);
  }
  isLoading.value = false;
};
</script>
