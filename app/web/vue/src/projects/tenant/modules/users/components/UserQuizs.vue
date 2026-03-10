<template>
  <div class="w-100 verify-card">
    <div class="">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("title.quiz") }} ( Score: {{ score }}/{{ total }} )
        </h2>
      </div>
      <div class="fv-row mb-10 ms-3" style="font-size: 16px">
        <div v-for="(item, index) in quiz" :key="index">
          <hr class="mt-9 mb-9" />

          <div class="questionTitle mb-7">
            {{ questions[item.id].question }}
          </div>

          <div
            class="d-flex mt-3"
            v-for="(option, val, index) in questions[item.id].options"
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

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { Field } from "vee-validate";
import questionDB from "@/core/data/wholesaleTest";
import clientAns from "@/core/data/wholesaleAns";

const questions = ref<any>(questionDB["en-us"]);
const score = ref(0);
const total = ref(0);
const props = defineProps<{
  verificationDetails: any;
}>();

const quiz = ref<any>({
  ...props.verificationDetails?.quiz,
});

onMounted(() => {
  total.value = Object.keys(quiz.value).length;
  score.value = Object.keys(quiz.value).length;

  for (const index in quiz.value) {
    if (quiz.value[index].answer != clientAns[quiz.value[index].id].answer) {
      score.value -= 1;
    }
  }

  if (total.value - score.value > 1) {
    score.value = total.value - 1;
  }
});

watch(
  () => props.verificationDetails,
  () => {
    quiz.value = {
      ...props.verificationDetails?.quiz,
    };
  }
);
</script>

<style scoped></style>
