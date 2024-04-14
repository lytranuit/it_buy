<template>
  <TreeSelect :options="departments" :normalizer="normalizer" :multiple="multiple" :modelValue="modelValue" :flat="flat"
    :name="name" :required="required" :value-consists-of="valueConsistsOf" :append-to-body="true"
    @update:modelValue="emit('update:modelValue', $event)" :disabled="disabled" :clearable="clearable"></TreeSelect>
</template>

<script setup>
// import TreeSelect from "vue3-acies-treeselect";
import { useAuth } from "../../stores/auth";
import { storeToRefs } from "pinia";
import { computed, onMounted, ref } from "vue";
import Api from "../../api/Api";
const props = defineProps({
  modelValue: {
    type: [Number, Array],
  },
  valueConsistsOf: String, //ALL_WITH_INDETERMINATE
  multiple: {
    type: Boolean,
    default: false,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  clearable: {
    type: Boolean,
    default: true,
  },
  required: {
    type: Boolean,
    default: false,
  },
  name: {
    type: String,
    default: "dep",
  },
  flat: {
    type: Boolean,
    default: true,
  },
});
const normalizer = (node) => {
  return {
    id: node.id,
    label: node.name,
  }
}
const emit = defineEmits(["update:modelValue"]);
const departments = ref([]);
onMounted(() => {
  Api.departmentsofuser().then((res) => {
    departments.value = res;
  });
});
</script>
