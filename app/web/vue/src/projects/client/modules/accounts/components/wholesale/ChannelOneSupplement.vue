<template>
  <div v-if="isLoading"></div>
  <div v-else class="card wholesale-process-card">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("wholesale.channelOneTitle_1") }}
        </h2>
      </div>

      <div class="fv-row mb-10 ms-3" style="font-size: 16px">
        <div v-for="(item, index) in questions" :key="index">
          <hr class="mt-9 mb-9" />

          <div class="questionTitle mb-7">{{ item.question }}</div>

          <div
            class="d-flex mt-3"
            v-for="(option, val, index) in item.options"
            :key="index"
          >
            <!--begin::Checkbox-->
            <div class="form-check form-check-custom form-check-solid mx-5">
              <Field
                v-model="item.answer"
                class="form-check-input"
                type="radio"
                :name="item.id.toString()"
                :value="val"
              />
            </div>
            <!--end::Checkbox-->
            <div>{{ option }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { useStore } from "@/store";
import { Field } from "vee-validate";
import { ref, onMounted } from "vue";
import clientAns from "@/core/data/wholesaleAns";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import questionDB from "@/core/data/wholesaleTest";
import i18n from "@/core/plugins/i18n";
import { getLanguage } from "@/core/types/LanguageTypes";

const { t, locale } = i18n.global;

const emits = defineEmits(["saved", "hasError"]);

const wrongAns = ref(0);
const store = useStore();
const isLoading = ref(false);
const questions = ref<any>({});
const answer = ref({});

// const language = ref(locale.value);

const handleStepSubmit = () => {
  answer.value = clientAns;

  wrongAns.value = 0;

  for (const index in questions.value) {
    if (
      questions.value[index].answer !=
      answer.value[questions.value[index].id].answer
    ) {
      wrongAns.value += 1;
    }
  }

  // console.log("Welcome to MyBCR", wrongAns.value);

  if (wrongAns.value > 1) {
    MsgPrompt.error(t("tip.wholesaleTestFail"), t("tip.testFailType")).then(
      () => {
        emits("saved", 3, {
          questions: questions.value.map(({ id, answer }) => ({ id, answer })),
          wrongAns: wrongAns.value,
        });
      }
    );
  } else {
    MsgPrompt.success(t("tip.testPass")).then(() => {
      emits("saved", 3, {
        questions: questions.value.map(({ id, answer }) => ({ id, answer })),
        wrongAns: wrongAns.value,
      });
    });
  }
};

const loadQuestions = () => {
  isLoading.value = true;
  questions.value = {};

  const questionProcess = structuredClone(questionDB[getLanguage.value]);

  const keys = Object.keys(questionProcess);
  const shuffledKeys = keys.sort(() => Math.random() - 0.5);
  const selectedKeys = shuffledKeys.slice(0, 10);

  questions.value = selectedKeys.map((key) => questionProcess[key]);
  isLoading.value = false;
};

onMounted(async () => {
  loadQuestions();
});

defineExpose({
  handleStepSubmit,
});
</script>

<style scoped>
.questionTitle {
  font-size: 17px;
  font-weight: 600;
}
</style>
