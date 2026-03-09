<template>
  <div
    class="modal fade"
    id="kt_modal_create_deposit"
    tabindex="-1"
    aria-hidden="true"
    ref="createPaymentMethodRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-900px">
      <div class="modal-content">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          style="max-height: 90vh; overflow: scroll"
        >
          <!------------------------------------------------------------------- Modal Header -->
          <div class="modal-header">
            <h2 class="fw-bold">Sophisticated Investor Test Record</h2>
            <div
              class="btn btn-icon btn-sm btn-active-icon-primary"
              data-bs-dismiss="modal"
            >
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <div class="p-7">
            <div class="fv-row mb-10 ms-3" style="font-size: 16px">
              <div v-for="(item, index) in questions" :key="index">
                <div class="questionTitle mb-4">
                  <strong
                    class="me-3"
                    :class="
                      item.ans != clientAns[item.id].answer
                        ? 'wrong'
                        : 'correct'
                    "
                    style="text-transform: uppercase"
                    >{{ item.ans }}</strong
                  >
                  <span>{{ item.question }}</span>
                </div>
                <div
                  class="ms-7 mb-7"
                  v-if="item.ans != clientAns[item.id].answer"
                >
                  <div
                    class="d-flex mt-3"
                    v-for="(option, val, index) in item.options"
                    :key="index"
                  >
                    <div
                      :class="
                        ['a', 'b', 'c', 'd'][index] == item.ans ? 'wrong' : ''
                      "
                    >
                      {{ option }}
                    </div>
                  </div>
                </div>
                <hr />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { showModal, hideModal } from "@/core/helpers/dom";
import clientAns from "@/core/data/wholesaleAns";
import questionDB from "@/core/data/wholesaleTest";

const quizRecord = ref<any>({});
const questions = ref<any>({});
const isLoading = ref(false);

const createPaymentMethodRef = ref<null | HTMLElement>(null);

const loadQuestions = () => {
  isLoading.value = true;
  questions.value = {};

  quizRecord.value.forEach((item) => {
    questions.value[item.id] = questionDB["en-us"][item.id];
    questions.value[item.id]["ans"] = item.answer;
  });
  isLoading.value = false;
};

const show = async (_QnA) => {
  quizRecord.value = _QnA;
  loadQuestions();
  showModal(createPaymentMethodRef.value);
};

const hide = () => {
  hideModal(createPaymentMethodRef.value);
};

defineExpose({
  hide,
  show,
});
</script>
<style scoped>
.correct {
  color: #14a44d;
}

.wrong {
  color: #dc4c64;
}
</style>
