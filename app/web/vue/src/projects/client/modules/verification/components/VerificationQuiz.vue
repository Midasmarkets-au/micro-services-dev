<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("title.quiz") }}
        </h2>
        <div class="text-dark ms-3">
          {{ $t("tip.quizDesc") }}
        </div>
      </div>
      <div class="fv-row mb-10 ms-3 answerText">
        <div v-for="(item, index) in questions" :key="index">
          <hr class="mt-9 mb-9" />

          <div class="questionTitle mb-7">{{ item.question }}</div>

          <div
            class="d-flex mt-3"
            v-for="(option, val, index) in item.options"
            :key="index"
          >
            <!--begin::Checkbox-->
            <div
              class="form-check form-check-custom form-check-solid mx-md-5 me-5"
            >
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
import superAns from "@/core/data/wholesaleTemp";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import questionDB from "@/core/data/wholesaleTest";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import i18n from "@/core/plugins/i18n";
import ican from "@/core/plugins/ICan";
import { useRouter } from "vue-router";
import { Actions } from "@/store/enums/StoreEnums";
import { getLanguage } from "@/core/types/LanguageTypes";

const { t } = i18n.global;
const emits = defineEmits(["saved", "hasError"]);

const props = defineProps<{
  data?: any;
  step: number;
}>();

const round = ref(0);
const questions = ref<any>(props.data || {});
const answer = ref({});
const wrongAns = ref(0);
const store = useStore();
const isLoading = ref(false);
const router = useRouter();
const score = ref(10);

const handleStepSubmit = async () => {
  round.value += 1;
  wrongAns.value = 0;
  score.value = 10;

  for (const index in questions.value) {
    if (
      questions.value[index].answer !=
      answer.value[questions.value[index].id].answer
    ) {
      wrongAns.value += 1;
      score.value -= 1;
    }
  }

  // console.log("Welcome to MyBCR", wrongAns.value);

  if (wrongAns.value > 1) {
    if (round.value < 2) {
      MsgPrompt.error(
        t("tip.testFail"),
        "Your Score: " + score.value + "/10"
      ).then(() => {
        loadQuestions();
        emits("hasError");
      });
    } else {
      await VerificationService.checkClientProfessionalAnswer({
        q1: questions.value[0].answer,
        q2: questions.value[1].answer,
        q3: questions.value[2].answer,
        q4: questions.value[3].answer,
        q5: questions.value[4].answer,
        q6: questions.value[5].answer,
        q7: questions.value[6].answer,
        q8: questions.value[7].answer,
        q9: questions.value[8].answer,
        q10: questions.value[9].answer,
        answerw: 10,
      });
      MsgPrompt.warning(
        t("tip.cantProcess"),
        "Your Score: " + score.value + "/10"
      ).then(async () => {
        await router.push({ name: "sign-in" });
        await store.dispatch(Actions.LOGOUT);
      });
    }
  } else {
    submitForm();
  }
};

const submitForm = async () => {
  const reducedObject = questions.value.map(({ id, answer }) => ({
    id,
    answer,
  }));

  try {
    await VerificationService.postVerificationQuiz(reducedObject);
    MsgPrompt.success(
      t("tip.testPass"),
      "Your Score: " + score.value + "/10"
    ).then(() => {
      emits("saved", props.step, {
        questions: reducedObject,
      });
    });
  } catch (error) {
    MsgPrompt.error(error);
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
  window.scrollTo(0, 0);
  isLoading.value = false;
};

const prepareData = () => {
  isLoading.value = true;
  questions.value = {};

  const questionProcess = questionDB[getLanguage.value];
  const keys = props.data.map((obj) => obj.id);

  questions.value = keys.map((key, index) => {
    return {
      id: key,
      question: questionProcess[key].question,
      answer: props.data[index].answer,
      options: questionProcess[key].options,
    };
  });

  isLoading.value = false;
};

onMounted(async () => {
  if (ican.cans(["SuperAdmin"])) {
    answer.value = superAns;
  } else {
    answer.value = clientAns;
  }

  if (props.data) {
    prepareData();
  } else {
    loadQuestions();
  }
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
.answerText {
  font-size: 16px;
}
@media (max-width: 768px) {
  .questionTitle {
    font-size: 15px;
  }
  .verify-card {
    padding-left: 20px;
    padding-right: 20px;
  }
  .answerText {
    font-size: 14px;
  }
}
</style>
