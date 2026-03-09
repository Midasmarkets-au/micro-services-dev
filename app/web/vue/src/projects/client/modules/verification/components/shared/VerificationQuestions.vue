<template>
  <div
    class="w-100 card mt-5"
    :style="{ display: hideQuestions ? 'none' : '' }"
  >
    <div class="card-header">
      <div class="card-title">{{ $t("tip.ansBelowQuestions") }}</div>
    </div>
    <div class="card-body question">
      <el-form-item
        v-for="(value, index) in questions"
        :label="
          {
            q1: $t('tip.verificationQ1'),
            q2: $t('tip.verificationQ2'),
            q3: $t('tip.verificationQ3'),
            q4: $t('tip.verificationQ4'),
          }[index]
        "
        :key="index"
        :prop="'questions.' + index"
      >
        <el-radio-group v-model="questions[index]" class="row w-100">
          <el-radio :label="true" :value="true" class="col-4 col-lg-2 ms-2">
            {{ $t("action.yes") }}
          </el-radio>

          <el-radio :label="false" :value="false" class="col-4 col-lg-2">
            {{ $t("action.no") }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { defineProps, defineModel } from "vue";

defineProps<{
  hideQuestions?: boolean;
}>();

const questions = defineModel<any>("questions", { required: true });
</script>

<style scoped lang="scss">
::v-deep .el-form-item__label {
  color: #3a3e44;
  font-size: 1rem;
}

::v-deep .question .el-form-item__label {
  font-weight: 500;
}

::v-deep .el-radio__input {
  .el-radio__inner {
    border: 1px solid #ccd3e0;
  }
}
</style>
