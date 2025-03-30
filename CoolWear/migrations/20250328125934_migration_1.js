/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.up = function (knex) {
  return knex.schema.alterTable('order', function (table) {
    // Remove is_refunded column
    table.dropColumn('is_refunded');

    // Add new columns
    table.string('customer_name').notNullable();
    table.string('customer_phone').notNullable();
    table.string('customer_address').notNullable();
    table.string('status').notNullable().defaultTo('Đang xử lý');

    // Make customer_id nullable (from required to optional)
    table.integer('customer_id').alter().nullable();
  });
};

/**
 * @param { import("knex").Knex } knex
 * @returns { Promise<void> }
 */
exports.down = function (knex) {
  return knex.schema.alterTable('order', function (table) {
    // Rollback: Remove new columns
    table.dropColumn('customer_name');
    table.dropColumn('customer_phone');
    table.dropColumn('customer_address');
    table.dropColumn('status');

    // Rollback: Add back is_refunded column
    table.boolean('is_refunded').notNullable().defaultTo(false);

    // Rollback: Make customer_id required again
    table.integer('customer_id').alter().notNullable();
  });
};
